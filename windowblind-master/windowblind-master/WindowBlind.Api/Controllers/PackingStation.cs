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
                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                data.ColumnNames.Add("CB Number");
                data.ColumnNames.Add("Line No");
                data.ColumnNames.Add("item");

                data.ColumnNames.Add("FabricCut-User");
                data.ColumnNames.Add("LogCut-User");
                data.ColumnNames.Add("EzStop-User");

                data.ColumnNames.Add("FabricCutDate-Time");
                data.ColumnNames.Add("LogCutDate-Time");
                data.ColumnNames.Add("EzStopDate-Time"); 

                var FabricCutterOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "FabricCut" && log.status == "Qualified");
                var LogCutOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "LogCut" && log.status == "Qualified");
                var EzStopOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "EzStop" && log.status == "Qualified");

                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();
                var LogCutOflogListModels = LogCutOflogModels.ToList();
                var EzStopOflogListModels = EzStopOflogModels.ToList();


                if (FabricCutterlistOflogModels.Count == 0 || LogCutOflogListModels.Count == 0 || EzStopOflogListModels.Count == 0) return new JsonResult(data);


                Dictionary<string, int> ProcessCounter = new Dictionary<string, int>();
                Dictionary<string, int> FabricCutterIndex = new Dictionary<string, int>();
                Dictionary<string, int> LogCutIndex = new Dictionary<string, int>();

                Dictionary<string, LogModel> FabricCutterDic = new Dictionary<string, LogModel>();
                Dictionary<string, LogModel> LogCutterDic = new Dictionary<string, LogModel>();

                int cntr = 0;
                foreach (var row in FabricCutterlistOflogModels)
                {
                    if (!ProcessCounter.ContainsKey(row.LineNumber))
                        ProcessCounter[row.LineNumber] = 0;
                    ProcessCounter[row.LineNumber]++;
                    FabricCutterIndex[row.LineNumber] = cntr++;
                    FabricCutterDic[row.LineNumber] = row;

                }

                cntr = 0;
                foreach (var row in LogCutOflogListModels)
                {
                    if (!ProcessCounter.ContainsKey(row.LineNumber))
                        ProcessCounter[row.LineNumber] = 0;
                    ProcessCounter[row.LineNumber]++;
                    LogCutIndex[row.LineNumber] = cntr++;
                    LogCutterDic[row.LineNumber] = row;

                }

                cntr = 0;
                foreach (var row in EzStopOflogListModels)
                {
                    if (!ProcessCounter.ContainsKey(row.LineNumber))
                        ProcessCounter[row.LineNumber] = 0;
                    ProcessCounter[row.LineNumber]++;

                    if (ProcessCounter[row.LineNumber] == 3)
                    {
                        row.row.Row["FabricCut-User"] = FabricCutterDic[row.LineNumber].UserName;
                        row.row.Row["LogCut-User"] = LogCutterDic[row.LineNumber].UserName;
                        row.row.Row["EzStop-User"] = row.UserName;
                        row.row.Row["FabricCutDate-Time"] = FabricCutterDic[row.LineNumber].dateTime;
                        row.row.Row["LogCutDate-Time"] = LogCutterDic[row.LineNumber].dateTime;
                        row.row.Row["EzStopDate-Time"] = row.dateTime;
                        data.Rows.Add(row.row);
                    }
                }

                return new JsonResult(data);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }


        [HttpPost("pushLinesNoToPackingStation")]
        public async Task<bool> pushLinesNoToPackingStation(CreateFileAndLabelModel model )
        {

            try
            {
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
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
