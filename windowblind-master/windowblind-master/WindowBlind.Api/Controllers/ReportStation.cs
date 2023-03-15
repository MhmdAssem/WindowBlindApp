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
    public class ReportStation : ControllerBase
    {

        public ReportStation(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;


        [HttpGet("GenerateReports")]
        public async Task<IActionResult> GenerateReports([FromHeader] string CBNumber)
        {

            try
            {

                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();
                data.ColumnNames.Add("Date-Time");
                data.ColumnNames.Add("CB Number");
                data.ColumnNames.Add("Line No");
                data.ColumnNames.Add("Customer");
                data.ColumnNames.Add("Hold Reasons");
                data.ColumnNames.Add("Admin_Notes");
                data.ColumnNames.Add("Status");
                Dictionary<string, int> LineNumberIndex = new Dictionary<string, int>();


                var FabricCutObjs = await _repository.Logs.FindAsync(log => log.ProcessType == "FabricCut" && log.CBNumber == CBNumber);
                var FabricCutObjsList = FabricCutObjs.ToList();

                GenerateStatusForLineNumber(ref data, FabricCutObjsList, "FabricCut", ref LineNumberIndex);


                var LogCutObjs = await _repository.Logs.FindAsync(log => log.ProcessType == "LogCut" && log.CBNumber == CBNumber);
                var LogCutObjsList = LogCutObjs.ToList();

                GenerateStatusForLineNumber(ref data, LogCutObjsList, "LogCut", ref LineNumberIndex);


                var EzStopObjs = await _repository.Logs.FindAsync(log => log.ProcessType == "EzStop" && log.CBNumber == CBNumber);
                var EzStopObjsList = EzStopObjs.ToList();

                GenerateStatusForLineNumber(ref data, EzStopObjsList, "EzStop", ref LineNumberIndex);


                var AssemblyObjs = await _repository.AssemblyStation.FindAsync(log => log.ProcessType == "Assembly" && log.CBNumber == CBNumber);
                var AssemblyObjsList = AssemblyObjs.ToList();

                GenerateStatusForLineNumber(ref data, AssemblyObjsList, "AssemblyStation", ref LineNumberIndex);

                var HoistObjs = await _repository.HoistStation.FindAsync(log => log.ProcessType == "Qualified" && log.CBNumber == CBNumber);
                var HoistObjsList = HoistObjs.ToList();

                GenerateStatusForLineNumber(ref data, HoistObjsList, "HoistStation", ref LineNumberIndex);

                var PackingObjs = await _repository.PackingStation.FindAsync(log => log.status == "Packed" && log.CBNumber == CBNumber);
                var PackingObjsList = PackingObjs.ToList();

                GenerateStatusForLineNumber(ref data, PackingObjsList, "PackingStation", ref LineNumberIndex);

                var HeldObjs = await _repository.Logs.FindAsync(log => log.status == "Held" && log.CBNumber == CBNumber);
                var HeldObjsList = HeldObjs.ToList();

                GenerateStatusForLineNumber(ref data, HeldObjsList, "", ref LineNumberIndex);


                return Ok(data);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }


        }
        private void GenerateStatusForLineNumber(ref FabricCutterCBDetailsModel Data, List<LogModel> list, string AppType, ref Dictionary<string, int> LineNumberIndex)
        {

            for (int i = 0; i < list.Count; i++)
            {
                if (!LineNumberIndex.ContainsKey(list[i].LineNumber))
                {
                    LineNumberIndex[list[i].LineNumber] = Data.Rows.Count;
                    if (AppType != "")
                        list[i].row.Row["Status"] = AppType + "-" + list[i].dateTime + " " + list[i].UserName + " " + list[i].TableName;
                    else
                        list[i].row.Row["Status"] = list[i].ProcessType + " -" + list[i].dateTime + " " + list[i].UserName + " " + list[i].TableName;
                    list[i].row.Row["Status"] = list[i].row.Row["Status"].Trim();
                    list[i].row.Row["Date-Time"] = DateTime.Now.ToString();
                    Data.Rows.Add(list[i].row);
                }
                else
                {
                    int ind = LineNumberIndex[list[i].LineNumber];
                    if (AppType != "")
                    {
                        Data.Rows[ind].Row["Status"] += "\n" + AppType + "-" + list[i].dateTime + " " + list[i].UserName + " " + list[i].TableName;
                        if(list[i].row.Row.ContainsKey("Hold Reasons"))
                        {
                            if (Data.Rows[ind].Row.ContainsKey("Hold Reasons"))
                                Data.Rows[ind].Row["Hold Reasons"] += "\n\n" + list[i].row.Row["Hold Reasons"];
                            else
                                Data.Rows[ind].Row["Hold Reasons"] = "\n\n" + list[i].row.Row["Hold Reasons"];
                        }
                    }
                    else
                    {
                        Data.Rows[ind].Row["Status"] += "\n" + list[i].ProcessType + " -" + list[i].dateTime + " " + list[i].UserName + " " + list[i].TableName;
                        if (list[i].row.Row.ContainsKey("Hold Reasons"))
                        {
                            if (Data.Rows[ind].Row.ContainsKey("Hold Reasons"))
                                Data.Rows[ind].Row["Hold Reasons"] += "\n\n" + list[i].row.Row["Hold Reasons"];
                            else
                                Data.Rows[ind].Row["Hold Reasons"] = "\n\n" + list[i].row.Row["Hold Reasons"];
                        }
                    }
                }
            }



        }
    }
}
