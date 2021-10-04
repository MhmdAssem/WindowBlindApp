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
    public class HoistStation : ControllerBase
    {
        public HoistStation(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;



        [HttpGet("GetReadyToQualify")]
        public async Task<IActionResult> GetReadyToQualify([FromHeader] string CbOrLineNumber)
        {
            try
            {

                var HoistColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "HoistStation").Result.FirstOrDefaultAsync();
                var HoistColumns = HoistColumnsSetting.settingPath.Split("@@@").ToList();
                if (HoistColumns[HoistColumns.Count - 1] == "") HoistColumns.RemoveAt(HoistColumns.Count - 1);


                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "CB Number");
                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "Line No");


                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

              

                var FabricCutterOflogModels = await _repository.AssemblyStation.FindAsync(log => log.ProcessType == "Assembly" && log.status == "Assembly" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));

                var lis = FabricCutterOflogModels.ToList();
                foreach (var row in lis)
                {
                     
                    row.row.Row["FromHoldingStation"] = "NO";
                    row.row.UniqueId = row.Id;
                    data.Rows.Add(row.row);
                }

                data.ColumnNames = HoistColumns;
                return new JsonResult(data);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }


        [HttpPost("pushLinesNoToHoistStation")]
        public async Task<bool> pushLinesNoToHoistStation(CreateFileAndLabelModel model)
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
                    log.TableName = model.tableName;
                    log.LineNumber = row.Row["Line No"];
                    log.CBNumber = row.Row["CB Number"];
                    log.Item = row.Row["item"];
                    log.dateTime = DateTime.Now.ToString();
                    log.status = "Qualified";
                    log.ProcessType = "Qualified";
                    log.Message = "This Line No.: " + log.LineNumber + ", has been Qualified at " + log.dateTime;
                    await _repository.HoistStation.InsertOneAsync(log);
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId, 
                        Builders<LogModel>.Update.Set(p => p.status, "Qualified"), new UpdateOptions { IsUpsert = false });
                    await _repository.AssemblyStation.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId,
                       Builders<LogModel>.Update.Set(p => p.status, "Qualified"), new UpdateOptions { IsUpsert = false });
                }

                return true;
            }
            catch (Exception)
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


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "Hoist" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {
                    Data.Rows.Add(item.Row);
                }

                var HoistColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "HoistStation").Result.FirstOrDefaultAsync();
                var HoistColumns = HoistColumnsSetting.settingPath.Split("@@@").ToList();
                if (HoistColumns[HoistColumns.Count - 1] == "") HoistColumns.RemoveAt(HoistColumns.Count - 1);
                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "CB Number");
                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "Line No");
                if (Data.Rows.Count != 0)
                    Data.ColumnNames = HoistColumns;

                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }

    }
}
