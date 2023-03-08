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
                    rejectModel.Row.Row["Admin_Notes"] = "";
                    rejectModel.Id = rejectModel.Row.UniqueId;
                    try
                    {
                        await _repository.Rejected.DeleteOneAsync(rej => rej.Id == oldId);
                    }
                    catch (Exception)
                    {

                    }

                    await InsertHeldObjectIntoLogs(rejectModel);
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
                        case "EzStop":
                            await _repository.EzStopData.DeleteOneAsync(e => e.id == oldId);
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


        public async Task InsertHeldObjectIntoLogs(RejectionModel model)
        {
            try
            {
                LogModel log = new LogModel();
                log.UserName = model.UserName;
                log.CBNumber = model.Row.Row["CB Number"];
                log.status = "Held";
                foreach (var key in model.Row.Row.Keys.ToList())
                {
                    if (key == "")
                    {
                        model.Row.Row.Remove(key); continue;
                    }
                    var ind = key.IndexOf(".");
                    if (ind == -1) continue;

                    var newKey = key.Replace(".", "");
                    var value = model.Row.Row[key];

                    model.Row.Row[newKey] = value;
                    model.Row.Row.Remove(key);
                }
                log.row = model.Row;
                log.LineNumber = model.Row.Row["Line No"];
                log.Item = "";
                log.dateTime = model.DateTime;
                log.Message = (model.Row.Row["CB Number"] + " " + model.Row.Row["Line No"] + " " + model.TableName + " " + model.UserName + " " + model.DateTime);
                log.ProcessType = "Held From " + model.StationName;
                log.TableName = model.TableName;
                log.Id = model.Id;
                await _repository.Logs.InsertOneAsync(log);

            }
            catch (Exception)
            {

            }

        }


        [HttpPost("UpdateReasonsForHeldObject")]
        public bool UpdateReasonsForHeldObject([FromBody] ReasonModel model)
        {

            try
            {
                var obj = _repository.Rejected.Find(e => e.Id == model.orderid).FirstOrDefault();

                if (model.addOrRemove)
                {
                    if (obj.RejectionReasons.FindIndex(e => e == model.reason) == -1)
                        obj.RejectionReasons.Add(model.reason);
                }
                else
                    obj.RejectionReasons.Remove(model.reason);

                _repository.Rejected.UpdateOne(rej => rej.Id == model.orderid,
                        Builders<RejectionModel>.Update.Set(p => p.RejectionReasons, obj.RejectionReasons), new UpdateOptions { IsUpsert = false });
                var row = _repository.Logs.Find(log => log.Id == model.orderid).FirstOrDefault().row.Row;
                row["Hold Reasons"] = model.originalStation + "\n " + String.Join("\n ", obj.RejectionReasons);
                row["Hold Reasons"] = row["Hold Reasons"].Trim();
                _repository.Logs.UpdateOne(log => log.Id == model.orderid,
                  Builders<LogModel>.Update.Set(p => p.row.Row, row), new UpdateOptions { IsUpsert = false });
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }



        [HttpPost("ClearOrdersFromHoldingStation")]
        public async Task<IActionResult> ClearOrdersFromHoldingStation([FromBody] List<RejectionModel> Model, [FromHeader] string UserName)
        {
            try
            {

                foreach (var rejected in Model)
                {
                    var item = rejected.Row;

                    await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                        Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Deleted By: " + UserName), new UpdateOptions { IsUpsert = false });

                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.Message);
            }

        }

        [HttpPost("SaveAdminNotes")]
        public async Task<IActionResult> SaveAdminNotes([FromBody] RejectionModel model)
        {
            try
            {
                await _repository.Rejected.UpdateOneAsync(rej => rej.Id == model.Id,
                        Builders<RejectionModel>.Update.Set(p => p.Row.Row, model.Row.Row), new UpdateOptions { IsUpsert = false });


                var row = _repository.Logs.Find(log => log.Id == model.Id).FirstOrDefault().row.Row;
                row["Admin_Notes"] = model.Row.Row["Admin_Notes"];
                
                _repository.Logs.UpdateOne(log => log.Id == model.Id,
                  Builders<LogModel>.Update.Set(p => p.row.Row, row), new UpdateOptions { IsUpsert = false });

                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.StackTrace);
            }

        }

    }
}
