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
using System.Net;
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


            CurentColumnMapper.Add("Control Type", "B/rail");

        }
        private IRepository _repository;
        private IWebHostEnvironment _env;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>(); /// this one maps the columns with the same values coming from Fabric/Logcut/ezstop
        Dictionary<string, string> CurentColumnMapper = new Dictionary<string, string>(); /// this one change the columns and values to new ones



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
                int Total = 0;
                string cbNumber = "";
                for (var i = 0; i < AssemblyColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(AssemblyColumns[i]))
                    {
                        AssemblyColumns[i] = CurentColumnMapper.ContainsKey(ColumnMapper[AssemblyColumns[i]]) ?CurentColumnMapper[ColumnMapper[AssemblyColumns[i]]] : ColumnMapper[AssemblyColumns[i]];
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
                    Total = int.Parse(row.row.Row["Total"]);
                    cbNumber = row.row.Row["CB Number"];
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
                    Total = int.Parse(row.row.Row["Total"]);
                    foreach (var col in CurentColumnMapper)
                    {
                        if (row.row.Row.ContainsKey(col.Key))
                        {
                            row.row.Row[col.Value] = row.row.Row[col.Key];
                        }
                    }
                    row.row.Row["Blind Number"] = (Convert.ToInt32(row.row.Row["Alpha"][0]) - 64).ToString();
                    data.Rows.Add(row.row);
                    cbNumber = row.row.Row["CB Number"];
                    LogCutterDic[row.LineNumber] = row;
                }

                cntr = 0;


                foreach (var row in EzStopOflogListModels)
                {
                    if (!ProcessCounter.ContainsKey(row.LineNumber))
                        ProcessCounter[row.LineNumber] = 0;
                    ProcessCounter[row.LineNumber]++;

                    if (ProcessCounter[row.LineNumber] >= 2)
                    {
                        //Total = int.Parse(row.row.Row["Total"]);
                        ProcessCounter[row.LineNumber] = -100000000;
                        row.row.Row["FromHoldingStation"] = "NO";
                        row.row.UniqueId = row.Id;
                        row.row.rows_AssociatedIds.Add(row.Id);
                        row.row.rows_AssociatedIds.Add(FabricCutterDic.ContainsKey(row.LineNumber) ? FabricCutterDic[row.LineNumber].Id : LogCutterDic[row.LineNumber].Id);
                        foreach (var col in CurentColumnMapper)
                        {
                            if (row.row.Row.ContainsKey(col.Key))
                            {
                                row.row.Row[col.Value] = row.row.Row[col.Key];
                            }
                        }
                        data.Rows.Add(row.row);
                        cbNumber = row.row.Row["CB Number"];
                    }
                }

                /// check if we have all data get the rows without the dep columns

                if (data.Rows.Count == Total && Total != 0)
                {
                    await AddRowsWithoutDepartment(data, cbNumber, Total);
                }

                data.ColumnNames = AssemblyColumns;
                data.ColumnNames.Insert(0, "Blind Number");

                return Repository.ReturnSuccessfulRequest(data);
            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }


        [HttpPost("pushLinesNoToAssemblyStation")]
        public async Task<IActionResult> pushLinesNoToAssemblyStation(CreateFileAndLabelModel model)
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
                    if (row.Row.ContainsKey("")) row.Row.Remove("");
                    log.UserName = model.userName;
                    log.LineNumber = row.Row["Line No"];
                    log.CBNumber = row.Row["CB Number"];
                    log.Item = row.Row.ContainsKey("item") ? row.Row["item"] : "NONE";
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

                return Repository.ReturnSuccessfulRequest(true);
            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
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

                return Repository.ReturnSuccessfulRequest(Data);

            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
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
                return Repository.ReturnSuccessfulRequest(true);
            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
            }

        }

        private async Task AddRowsWithoutDepartment(FabricCutterCBDetailsModel data, string cbNumber, int Total)
        {
            /// get the dumb file Path
            var ctbsodumpSetting = await _repository.Settings.FindAsync(e => e.settingName == "ctbsodump");
            var ctbsodumpPath = ctbsodumpSetting.FirstOrDefault().settingPath;

            /// get the sheet name
            var SheetNameSetting = await _repository.Settings.FindAsync(e => e.settingName == "SheetName");
            var SheetNamePath = SheetNameSetting.FirstOrDefault().settingPath;

            ctbsodumpPath = Repository.CreateNewFile(ctbsodumpPath, ctbsodumpPath.Substring(0, ctbsodumpPath.IndexOf(".")) + Guid.NewGuid().ToString() + ctbsodumpPath.Substring(ctbsodumpPath.IndexOf(".")));

            FileInfo file = new FileInfo(ctbsodumpPath);
            using (var package = new ExcelPackage(file))
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Where(e => e.Name == SheetNamePath).FirstOrDefault();
                if (worksheet == null) new JsonResult(false);
                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;
                int index = -1;
                for (int i = start.Column; i <= end.Column && index == -1; i++)
                {
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    if (Headertext == "W/Order NO")
                        for (int j = start.Row + 1; j <= end.Row && index == -1; j++)
                        {
                            var text = worksheet.Cells[j, i].Text.Trim();

                            if (text == cbNumber)
                                index = j;
                        }
                }

                for (int j = index; j <= end.Row; j++)
                {
                    FabricCutterCBDetailsModelTableRow NewRow = new FabricCutterCBDetailsModelTableRow();
                    bool found = false;
                    for (int i = start.Column; i <= end.Column; i++)
                    {
                        var Headertext = worksheet.Cells[1, i].Text.Trim();
                        Headertext = Headertext.Replace(".", "");
                        if (String.IsNullOrEmpty(Headertext)) continue;

                        var cell = worksheet.Cells[j, i].Text.Trim();
                        if (cell != "" && Headertext == "Department") break;

                        if (Headertext == "W/Order NO" && cell != cbNumber)
                        {
                            System.IO.File.Delete(ctbsodumpPath);
                            return;
                        }
                        if (Headertext == "Name-key" && cell == "")
                        {
                            found = false; /// skip this row with no Name-Key
                            break;
                        }

                        if (ColumnMapper.ContainsKey(Headertext))
                        {
                            NewRow.Row[Headertext] = cell;
                            Headertext = ColumnMapper[Headertext];
                        }
                        NewRow.Row[Headertext] = cell;
                        found = true;
                    }
                    if (found)
                    {
                        NewRow.Row["FromHoldingStation"] = "";
                        NewRow.Row["Total"] = Total.ToString();
                        NewRow.UniqueId = Guid.NewGuid().ToString();
                        data.Rows.Add(NewRow);
                    }
                }
            }
            System.IO.File.Delete(ctbsodumpPath);


        }

    }
}
