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


            if (ColumnMapper.Count > 0) return;
            ColumnMapper.Add("Customer Name 1", "Customer");
            ColumnMapper.Add("W/Order NO", "CB Number");
            ColumnMapper.Add("Qty", "Quantity");
            ColumnMapper.Add("Width", "Measured Width");
            ColumnMapper.Add("Drop", "Measured Drop");
            ColumnMapper.Add("Fabric", "Fabric Type");
            ColumnMapper.Add("Colour", "Fabric Colour");
            ColumnMapper.Add("Pull Type / Control Type /Draw Type", "Control Type");

        }
        private IRepository _repository;
        private IWebHostEnvironment _env;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();


        [HttpGet("GetReadyToAssemble")]
        public async Task<IActionResult> GetReadyToAssemble([FromHeader] string CbOrLineNumber)
        {
            try
            {
                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                var FabricCutterOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "FabricCut" && log.status == "IDLE" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));
                var LogCutOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "LogCut" && log.status == "IDLE" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));
                var EzStopOflogModels = await _repository.Logs.FindAsync(log => log.ProcessType == "EzStop" && log.status == "IDLE" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));

                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();
                var LogCutOflogListModels = LogCutOflogModels.ToList();
                var EzStopOflogListModels = EzStopOflogModels.ToList();

                var AssemblyColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "AssemblyStation").Result.FirstOrDefaultAsync();
                var AssemblyColumns = AssemblyColumnsSetting.settingPath.Split("@@@").ToList();
                if (AssemblyColumns[AssemblyColumns.Count - 1] == "") AssemblyColumns.RemoveAt(AssemblyColumns.Count - 1);

                for (var i = 0; i < AssemblyColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(AssemblyColumns[i]))
                    {
                        AssemblyColumns[i] = ColumnMapper[AssemblyColumns[i]];
                    }
                }


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

                Dictionary<string, int> TakenFromLogCut = new Dictionary<string, int>();
                cntr = 0;
                foreach (var row in LogCutOflogListModels)
                {
                    if (TakenFromLogCut.ContainsKey(row.LineNumber))
                        continue;
                    TakenFromLogCut[row.LineNumber] = 0;
                    row.row.Row["FromHoldingStation"] = "NO";
                    row.row.UniqueId = row.Id;
                    row.row.rows_AssociatedIds.Add(row.Id);
                   
                    data.Rows.Add(row.row);
                }

                cntr = 0;

                foreach (var row in EzStopOflogListModels)
                {
                    if (!ProcessCounter.ContainsKey(row.LineNumber))
                        ProcessCounter[row.LineNumber] = 0;
                    ProcessCounter[row.LineNumber]++;

                    if (ProcessCounter[row.LineNumber] >= 2)
                    {
                        ProcessCounter[row.LineNumber] = -100000000;
                        row.row.Row["FromHoldingStation"] = "NO";
                        row.row.UniqueId = row.Id;
                        row.row.rows_AssociatedIds.Add(row.Id);
                        //row.row.rows_AssociatedIds.Add(LogCutterDic[row.LineNumber].Id);
                        row.row.rows_AssociatedIds.Add(FabricCutterDic[row.LineNumber].Id);
                        data.Rows.Add(row.row);
                    }
                }


                data.ColumnNames = AssemblyColumns;
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
                    log.status = "Assembly";
                    log.ProcessType = "Assembly";
                    log.TableName = model.tableName;
                    log.Message = "This Line No.: " + log.LineNumber + ", has been assembled at " + log.dateTime;
                    await _repository.AssemblyStation.InsertOneAsync(log);

                    foreach (var id in row.rows_AssociatedIds)
                    {
                        await _repository.Logs.UpdateOneAsync(log => log.Id == id,
                                                                Builders<LogModel>.Update.Set(p => p.status, "Assembly"), new UpdateOptions { IsUpsert = false });
                    }


                }

                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }


        [HttpGet("GetHeldObjects")]
        public async Task<IActionResult> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "Assembly" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {
                    Data.Rows.Add(item.Row);
                }

                var AssemblyColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "AssemblyStation").Result.FirstOrDefaultAsync();
                var AssemblyColumns = AssemblyColumnsSetting.settingPath.Split("@@@").ToList();
                if (AssemblyColumns[AssemblyColumns.Count - 1] == "") AssemblyColumns.RemoveAt(AssemblyColumns.Count - 1);
                
                for (var i = 0; i < AssemblyColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(AssemblyColumns[i]))
                    {
                        AssemblyColumns[i] = ColumnMapper[AssemblyColumns[i]];
                    }
                }
                //AssemblyColumns = LogCut.AddColumnIfNotExists(AssemblyColumns, "CB Number");
                // AssemblyColumns = LogCut.AddColumnIfNotExists(AssemblyColumns, "Line No");
                if (Data.Rows.Count != 0)
                    Data.ColumnNames = AssemblyColumns;

                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }

        [HttpPost("ClearOrdersFromAssembly")]
        public async Task<IActionResult> ClearOrdersFromAssembly([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                foreach (var item in model.data.Rows)
                {
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });
                    foreach (var id in item.rows_AssociatedIds)
                    {
                        await _repository.Logs.UpdateOneAsync(log => log.Id == id,
                                                                Builders<LogModel>.Update.Set(p => p.status, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });
                    }
                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.Message);
            }

        }
    }
}
