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
        public async Task<IActionResult> GetReadyToQualify()
        {
            try
            {
                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                data.ColumnNames.Add("CB Number");
                data.ColumnNames.Add("Line No");
                data.ColumnNames.Add("item");

                data.ColumnNames.Add("Assembly User");


                data.ColumnNames.Add("Assembly Date-Time");

                var FabricCutterOflogModels = await _repository.AssemblyStation.FindAsync(log => log.ProcessType == "Assembly" && log.status == "Assembly");

                var lis = FabricCutterOflogModels.ToList();
                foreach (var row in lis)
                {
                    row.row.Row["Assembly User"] = row.UserName;
                    row.row.Row["Assembly Date-Time"] = row.dateTime;
                    data.Rows.Add(row.row);
                }

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
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"],
                        Builders<LogModel>.Update.Set(p => p.status, "Qualified"), new UpdateOptions { IsUpsert = false });
                    await _repository.AssemblyStation.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"],
                       Builders<LogModel>.Update.Set(p => p.status, "Qualified"), new UpdateOptions { IsUpsert = false });
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
