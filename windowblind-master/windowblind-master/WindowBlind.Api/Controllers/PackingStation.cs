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

                data.ColumnNames.Add("Hoisting User");


                data.ColumnNames.Add("Hoist Date Time");


                var FabricCutterOflogModels = await _repository.HoistStation.FindAsync(log => log.ProcessType == "Qualified" && log.status == "Qualified");

                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();



                if (FabricCutterlistOflogModels.Count == 0) return new JsonResult(data);


                foreach (var row in FabricCutterlistOflogModels)
                {

                    row.row.Row["Hoisting User"] = row.UserName;
                    row.row.Row["Hoist Date Time"] = row.dateTime;
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
