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
        private static Dictionary<string, int> CBNumberCounter;


        public async Task CheckCBNumber()
        {
            if (CBNumberCounter == null)
                CBNumberCounter = await GetCbNumberCountFromFile();
        }

        [HttpGet("GetReadyToPack")]
        public async Task<IActionResult> GetReadyToPack([FromHeader] string CbOrLineNumber)
        {
            try
            {
                await CheckCBNumber();

                var PackingColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "PackingStation").Result.FirstOrDefaultAsync();
                var PackingColumns = PackingColumnsSetting.settingPath.Split("@@@").ToList();
                if (PackingColumns[PackingColumns.Count - 1] == "") PackingColumns.RemoveAt(PackingColumns.Count - 1);

                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "CB Number");
                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "Line No");

                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                // lets assume this was line number
                var FabricCutterOflogModels = await _repository.HoistStation.FindAsync(log => log.ProcessType == "Qualified" && log.status == "Qualified" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));
                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();



                foreach (var row in FabricCutterlistOflogModels)
                {

                  
                    row.row.Row["FromHoldingStation"] = "NO";

                    row.row.Row["Total"] = CBNumberCounter[row.CBNumber].ToString();
                    row.row.UniqueId = row.Id;
                    var RowsWithTheSameCBNumber = _repository.PackingStation.Find(pack => pack.CBNumber == row.CBNumber && pack.status == "Packed").ToList();

                    if (RowsWithTheSameCBNumber == null)
                    {
                        row.row.Row["Packed"] = "0";
                    }
                    else
                    {
                        row.row.Row["Packed"] = RowsWithTheSameCBNumber.Count.ToString() ;
                    }

                    data.Rows.Add(row.row);

                }



                data.ColumnNames = PackingColumns;
                data.ColumnNames.Add("Status");

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
                await CheckCBNumber();
                foreach (var row in model.data.Rows)
                {

                    if (row.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == row.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });

                    var log = new LogModel();
                    log.row = row;
                    log.UserName = model.userName;
                    log.LineNumber = row.Row["Line No"];
                    log.CBNumber = row.Row["CB Number"];
                    log.Item = row.Row["item"];
                    log.dateTime = DateTime.Now.ToString();
                    log.status = "Packed";
                    log.ProcessType = row.PackingType;
                    log.TableName = model.tableName;
                    log.Message = "This Line No.: " + log.LineNumber + ", has been Qualified at " + log.dateTime;
                    await _repository.PackingStation.InsertOneAsync(log);
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId,
                         Builders<LogModel>.Update.Set(p => p.status, "Packed"), new UpdateOptions { IsUpsert = false });
                    await _repository.HoistStation.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId,
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

        public string CreateNewFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
            return destination;

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

            ctbsodumpPath = CreateNewFile(ctbsodumpPath, ctbsodumpPath.Substring(0, ctbsodumpPath.IndexOf(".")) + Guid.NewGuid().ToString() + ctbsodumpPath.Substring(ctbsodumpPath.IndexOf(".")));

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
            System.IO.File.Delete(ctbsodumpPath);
            return CBNumberCounter;

        }

        [HttpGet("GetHeldObjects")]
        public async Task<IActionResult> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "Packing" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {
                    Data.Rows.Add(item.Row);
                }

                var PackingColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "PackingStation").Result.FirstOrDefaultAsync();
                var PackingColumns = PackingColumnsSetting.settingPath.Split("@@@").ToList();
                if (PackingColumns[PackingColumns.Count - 1] == "") PackingColumns.RemoveAt(PackingColumns.Count - 1);

                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "CB Number");
                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "Line No");
                if (Data.Rows.Count != 0)
                    Data.ColumnNames = PackingColumns;

                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }
    }
}
