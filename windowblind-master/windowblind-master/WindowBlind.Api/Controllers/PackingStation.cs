using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowBlind.Api.Models;
namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PackingStation : ControllerBase
    {

        public PackingStation(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;



        [HttpGet("GetReadyToPack")]
        public async Task<IActionResult> GetReadyToPack()
        {
            try
            {
                Dictionary<string, int> CBNumberCounter = await GetCbNumberCountFromFile();
                Dictionary<string, int> PackedFileCBNumberCounter = new Dictionary<string, int>();


                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                data.ColumnNames.Add("CB Number");
                data.ColumnNames.Add("Line No");
                data.ColumnNames.Add("item");
                data.ColumnNames.Add("Hoisting User");
                data.ColumnNames.Add("Hoist Date Time");
                data.ColumnNames.Add("Status");


                var FabricCutterOflogModels = await _repository.HoistStation.FindAsync(log => log.ProcessType == "Qualified" && log.status == "Qualified");

                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();



                if (FabricCutterlistOflogModels.Count == 0) return new JsonResult(data);

                foreach (var row in FabricCutterlistOflogModels)
                {
                    if (!PackedFileCBNumberCounter.ContainsKey(row.CBNumber))
                        PackedFileCBNumberCounter[row.CBNumber] = 0;
                    PackedFileCBNumberCounter[row.CBNumber]++;
                }


                foreach (var row in FabricCutterlistOflogModels)
                {

                    row.row.Row["Hoisting User"] = row.UserName;
                    row.row.Row["Hoist Date Time"] = row.dateTime;
                    if (PackedFileCBNumberCounter[row.CBNumber] == CBNumberCounter[row.CBNumber])
                    {
                        row.row.Row["Status"] = "Dispatch";
                    }
                    else
                    {
                        var RowsWithTheSameCBNumber = _repository.PackingStation.Find(pack => pack.CBNumber == row.CBNumber && pack.status == "Packed").ToList();

                        if (RowsWithTheSameCBNumber == null)
                        {
                            row.row.Row["Status"] = "Holding bay";
                        }
                        else if (RowsWithTheSameCBNumber.Count + PackedFileCBNumberCounter[row.CBNumber] == CBNumberCounter[row.CBNumber])
                        {
                            row.row.Row["Status"] = "Dispatch via holding bay";
                        }
                        else
                        {
                            row.row.Row["Status"] = "Holding bay";
                        }
                    }
                    data.Rows.Add(row.row);

                }

                return new JsonResult(data);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }


        [HttpPost("pushLinesNoToPackingStation")]
        public async Task<bool> pushLinesNoToPackingStation(CreateFileAndLabelModel model)
        {

            try
            {
                Dictionary<string, int> CBNumberCounter = await GetCbNumberCountFromFile();
                foreach (var row in model.data.Rows)
                {
                    var log = new LogModel();
                    log.row = row;
                    log.UserName = model.userName;
                    log.LineNumber = row.Row["Line No"];
                    log.CBNumber = row.Row["CB Number"];
                    log.Item = row.Row["item"];
                    log.dateTime = DateTime.Now.ToString();
                    log.status = "Packed";
                    log.ProcessType = row.PackingType;
                    log.Message = "This Line No.: " + log.LineNumber + ", has been Qualified at " + log.dateTime;
                    await _repository.PackingStation.InsertOneAsync(log);
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"],
                         Builders<LogModel>.Update.Set(p => p.status, "Packed"), new UpdateOptions { IsUpsert = false });
                    await _repository.HoistStation.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"],
                        Builders<LogModel>.Update.Set(p => p.status, "Packed"), new UpdateOptions { IsUpsert = false });
                }
                var cbNumbersRowsInPackingDB = await _repository.PackingStation.FindAsync(log => log.CBNumber == model.data.Rows[0].Row["CB Number"] && log.status == "Packed").Result.ToListAsync();

                if (cbNumbersRowsInPackingDB != null)
                {
                    var count = cbNumbersRowsInPackingDB.Count;
                    if (count == CBNumberCounter[cbNumbersRowsInPackingDB[0].CBNumber])
                    {
                        await _repository.PackingStation.UpdateManyAsync(log => log.CBNumber == cbNumbersRowsInPackingDB[0].CBNumber,
                      Builders<LogModel>.Update.Set(p => p.status, model.data.Rows[0].Row["Status"]), new UpdateOptions { IsUpsert = false });
                    }
                }


                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }


        private async Task<Dictionary<string, int>> GetCbNumberCountFromFile()
        {
            Dictionary<string, int> CBNumberCounter = new Dictionary<string, int>();

            /// get the dumb file Path
            var ctbsodumpSetting = await _repository.Settings.FindAsync(e => e.settingName == "ctbsodump");
            var ctbsodumpPath = ctbsodumpSetting.FirstOrDefault().settingPath;

            /// get the sheet name
            var SheetNameSetting = await _repository.Settings.FindAsync(e => e.settingName == "SheetName");
            var SheetNamePath = SheetNameSetting.FirstOrDefault().settingPath;


            FileInfo file = new FileInfo(ctbsodumpPath);
            using (var package = new ExcelPackage(file))
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Where(e => e.Name == SheetNamePath).FirstOrDefault();
                if (worksheet == null) new JsonResult(false);
                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;
                for (int i = start.Column; i < end.Column; i++)
                {
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    if (Headertext == "W/Order NO")
                        for (int j = start.Row + 1; j < end.Row; j++)
                        {
                            var text = worksheet.Cells[j, i].Text.Trim();
                            if (!CBNumberCounter.ContainsKey(text)) CBNumberCounter[text] = 0;

                            CBNumberCounter[text]++;

                        }
                }
            }
            return CBNumberCounter;

        }
    }
}
