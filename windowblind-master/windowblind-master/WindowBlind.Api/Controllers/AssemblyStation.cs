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
    public class AssemblyStation : ControllerBase
    {

        public AssemblyStation(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;



        [HttpGet("GetReadyToAssemble")]
        public async Task<IActionResult> GetReadyToAssemble()
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

                var FabricCutterOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "FabricCut" && log.status == "IDLE");
                var LogCutOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "LogCut" && log.status == "IDLE");
                var EzStopOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "EzStop" && log.status == "IDLE");

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

                    if (ProcessCounter[row.LineNumber] >= 3)
                    {
                        ProcessCounter[row.LineNumber] = -100000000;
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


        [HttpPost("pushLinesNoToAssemblyStation")]
        public async Task<bool> pushLinesNoToAssemblyStation(CreateFileAndLabelModel model)
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
                    log.status = "Assembly";
                    log.ProcessType = "Assembly";
                    log.Message = "This Line No.: " + log.LineNumber + ", has been assembled at " + log.dateTime;
                    await _repository.AssemblyStation.InsertOneAsync(log);
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"],
                        Builders<LogModel>.Update.Set(p => p.status, "Assembly"), new UpdateOptions { IsUpsert = false });

                }

                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }

    }
}
