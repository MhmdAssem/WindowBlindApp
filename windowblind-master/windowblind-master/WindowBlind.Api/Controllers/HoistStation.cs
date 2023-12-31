﻿using Microsoft.AspNetCore.Hosting;
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
      
        private IRepository _repository;
        private IWebHostEnvironment _env;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();

        public HoistStation(IRepository repository, IWebHostEnvironment env)
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

        [HttpGet("GetReadyToQualify")]
        public async Task<ResultModel> GetReadyToQualify([FromHeader] string CbOrLineNumber)
        {
            try
            {

                var HoistColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "HoistStation").Result.FirstOrDefaultAsync();
                var HoistColumns = HoistColumnsSetting.settingPath.Split("@@@").ToList();
                if (HoistColumns[HoistColumns.Count - 1] == "") HoistColumns.RemoveAt(HoistColumns.Count - 1);

                for (var i = 0; i < HoistColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(HoistColumns[i]))
                    {
                        HoistColumns[i] = ColumnMapper[HoistColumns[i]];
                    }
                }

                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "CB Number");
                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "Line No");


                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

              

                var FabricCutterOflogModels = await _repository.AssemblyStation.FindAsync(log => log.ProcessType == "Assembly" && log.status == "Assembly" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));

                var lis = FabricCutterOflogModels.ToList();
                foreach (var row in lis)
                {
                    row.row.rows_AssociatedIds.Clear();
                    row.row.Row["FromHoldingStation"] = "NO";
                    row.row.UniqueId = row.Id;
                    row.row.rows_AssociatedIds.Add(row.Id);
                    row.row.Row["Blind Number"] = (Convert.ToInt32(row.row.Row["Alpha"][0]) - 64).ToString();
                    data.Rows.Add(row.row);
                }

                data.ColumnNames = HoistColumns;
                data.ColumnNames.Insert(0, "Blind Number");
                return Repository.ReturnSuccessfulRequest(data);
            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
            }
        }


        [HttpPost("pushLinesNoToHoistStation")]
        public async Task<ResultModel> pushLinesNoToHoistStation(CreateFileAndLabelModel model)
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
                    log.Item =row.Row.ContainsKey("item")? row.Row["item"]:"NONE";
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

                return Repository.ReturnSuccessfulRequest(true);
            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
            }
        }


        [HttpGet("GetHeldObjects")]
        public async Task<ResultModel> GetHeldObjects([FromHeader] string tableName)
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


                for (var i = 0; i < HoistColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(HoistColumns[i]))
                    {
                        HoistColumns[i] = ColumnMapper[HoistColumns[i]];
                    }
                }

                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "CB Number");
                //HoistColumns = LogCut.AddColumnIfNotExists(HoistColumns, "Line No");
                if (Data.Rows.Count != 0)
                    Data.ColumnNames = HoistColumns;

                return Repository.ReturnSuccessfulRequest(Data);

            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }


        [HttpPost("ClearOrdersFromHoist")]
        public async Task<ResultModel> ClearOrdersFromHoist([FromBody] CreateFileAndLabelModel model)
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
                        await _repository.AssemblyStation.UpdateOneAsync(log => log.Id == id,
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
    }
}
