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
    public class HoldingStation : ControllerBase
    {
        public HoldingStation(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;


        [HttpGet("GetAllRejectedOrders")]

        public async Task<IActionResult> GetAllRejectedOrders()
        {
            try
            {

                var Rejected = await _repository.Rejected.FindAsync(e => e.ForwardedToStation == "Admin");

                if (Rejected != null)
                    return new JsonResult(Rejected.ToList());

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpPost("RejectThisRow")]

        public async Task<bool> RejectThisRow([FromBody] List<RejectionModel> model)
        {

            try
            {
                foreach (var rejectModel in model)
                {
                    var oldId = rejectModel.Row.UniqueId;
                    rejectModel.Row.UniqueId = Guid.NewGuid().ToString();
                    rejectModel.Id = rejectModel.Row.UniqueId;


                    try
                    {
                         await _repository.Rejected.DeleteOneAsync(rej => rej.Id == oldId);
                    }
                    catch (Exception)
                    {

                    }


                    await _repository.Rejected.InsertOneAsync(rejectModel);

                    switch (rejectModel.StationName)
                    {
                        case "Assembly":
                            await _repository.Logs.DeleteOneAsync(a => a.LineNumber == rejectModel.Row.Row["Line No"] && a.Id == rejectModel.Id && a.status == "IDLE");
                            break;
                        case "Hoist":
                            await _repository.AssemblyStation.DeleteOneAsync(a => a.LineNumber == rejectModel.Row.Row["Line No"] && a.Id == rejectModel.Id && a.ProcessType == "Assembly" && a.status == "Assembly");
                            break;
                        case "Packing":
                            await _repository.HoistStation.DeleteOneAsync(a => a.LineNumber == rejectModel.Row.Row["Line No"] && a.Id == rejectModel.Id && a.ProcessType == "Qualified" && a.status == "Qualified");
                            break;
                        default:
                            break;
                    }

                }


                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }


        [HttpPost("ApproveThisOrders")]

        public async Task<bool> ApproveThisOrders([FromBody] OrdersApprovalModel model)
        {
            try
            {
                foreach (var rejectModel in model.data)
                {
                    rejectModel.Row.Row["FromHoldingStation"] = "YES";
                    await _repository.Rejected.UpdateOneAsync(rej => rej.Id == rejectModel.Id,
                        Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, model.forwardStation), new UpdateOptions { IsUpsert = false });
                    await _repository.Rejected.UpdateOneAsync(rej => rej.Id == rejectModel.Id,
                      Builders<RejectionModel>.Update.Set(p => p.Row, rejectModel.Row), new UpdateOptions { IsUpsert = false });
                    await _repository.Rejected.UpdateOneAsync(rej => rej.Id == rejectModel.Id,
                                          Builders<RejectionModel>.Update.Set(p => p.TableName, rejectModel.TableName), new UpdateOptions { IsUpsert = false });

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
