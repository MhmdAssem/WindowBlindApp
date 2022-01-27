using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        public SettingsController(IRepository repository)
        {
            Repository = repository;
        }
        private IRepository Repository;
        // GET: api/<TablesController>
        [HttpGet("GetAllSettings")]
        public async Task<List<FileSetting>> GetAllSettings()
        {
            try
            {
                var settings = await this.Repository.Settings.FindAsync(e => e.settingName != "").Result.ToListAsync();
                return settings;
            }
            catch (Exception e)
            {

                throw;
            }

        }


        [HttpGet("getColumnsNames")]
        public List<string> getColumnsNames()
        {
            try
            {
                string Path = Repository.Settings.Find(s => s.settingName == "ctbsodump").FirstOrDefault().settingPath;
                string SheetName = Repository.Settings.Find(s => s.settingName == "SheetName").FirstOrDefault().settingPath;

                FileInfo file = new FileInfo(Path);
                

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                List<string> names = new List<string>();
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.Where(e => e.Name == SheetName).FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;

                    for (int i = start.Column; i <= end.Column; i++)
                    {
                        var text = worksheet.Cells[1, i].Text.Trim();
                        text = text.Replace(".", "");
                        if (text != "")
                            names.Add(text);
                    }


                    package.Dispose();
                }
                return names;
            }
            catch (Exception e)
            {
                List<string> error = new List<string>();
                error.Add(e.Message);
                error.Add(e.StackTrace);

                return error;
            }

        }

        [HttpGet("GetPrinterNames")]

        public List<string> GetPrinterNames()
        {
            try
            {
                List<string> res = new List<string>();
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    res.Add(printer);
                }
                return res;
            }
            catch (Exception e)
            {

                return new List<string>();
            }
        }

        [HttpPost("UpdateAllSettings")]
        public async Task<bool> UpdateAllSettings([FromBody] List<FileSetting> settings)
        {
            try
            {
                foreach (var setting in settings)
                {
                    await this.Repository.Settings.ReplaceOneAsync(set => set.settingName == setting.settingName && set.applicationSetting == setting.applicationSetting, setting);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }

        [HttpGet("getTableNumber")]
        public async Task<IActionResult> getTableNumber([FromHeader] string name)
        {
            try
            {
                var value = await this.Repository.Settings.FindAsync(set => set.settingName == name).Result.FirstOrDefaultAsync();
                return new JsonResult(value.settingPath);
            }
            catch (Exception)
            {

                return new JsonResult("false");
            }

        }


        [HttpGet("GetSearchType")]
        public async Task<IActionResult> GetSearchType()
        {
            try
            {
                var val = await Repository.Settings.FindAsync(setting => setting.settingName == "SearchType").Result.FirstOrDefaultAsync();

                return new JsonResult(val.settingPath == "CB_Line_Number_Search");
            }
            catch (Exception e)
            {

                return new JsonResult(null);
            }

        }


        [HttpGet("CheckTableExists")]
        public async Task<IActionResult> CheckTableExists([FromHeader] string name)
        {
            try
            {
                var printers = await Repository.Tables.FindAsync(set => set.TableName == name).Result.ToListAsync();
                if (printers == null || printers.Count == 0)
                    return new JsonResult(true);

                return new JsonResult(false);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }

        }


        [HttpGet("InsertTableNameWithThePath")]
        public async Task<IActionResult> InsertTableNameWithThePath([FromHeader] string TableName, [FromHeader] string OutputPath)
        {
            try
            {

                ApplicationTableWithOutputPath app = new ApplicationTableWithOutputPath
                {
                    Id = Guid.NewGuid().ToString(),
                    OutputPath = OutputPath,
                    TableName = TableName
                };

                var entry = await Repository.Tables.FindAsync(table => table.TableName == TableName).Result.FirstOrDefaultAsync();
                if (entry == null)
                    await Repository.Tables.InsertOneAsync(app);

                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }

        }


        [HttpGet("GetTablesBasedOnApplication")]

        public async Task<IActionResult> GetTablesBasedOnApplication([FromHeader] string application)
        {
            try
            {
                List<string> res = new List<string>();

                string applicationEntry = "";

                switch (application)
                {
                    case "FabricCut":
                        var temp = await Repository.Settings.FindAsync(setting => setting.settingName == "FabricCutterTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    case "LogCut":
                        temp = await Repository.Settings.FindAsync(setting => setting.settingName == "LogCutterTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    case "EzStop":
                        temp = await Repository.Settings.FindAsync(setting => setting.settingName == "EzStopTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    case "Assembly":
                        temp = await Repository.Settings.FindAsync(setting => setting.settingName == "AssemblyStationTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    case "Hoist":
                        temp = await Repository.Settings.FindAsync(setting => setting.settingName == "HoistStationTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    case "Packing":
                        temp = await Repository.Settings.FindAsync(setting => setting.settingName == "PackingStationTable").Result.FirstOrDefaultAsync();
                        applicationEntry = temp.settingPath;
                        break;
                    default:
                        break;
                }


                foreach (string entry in applicationEntry.Split("#####"))
                {
                    var table = entry.Split("@@@@@")[1];
                    res.Add(table);
                }
                return new JsonResult(res);
            }
            catch (Exception e)
            {

                return new JsonResult(new List<string>());
            }
        }

        [HttpGet("DeleteTable")]
        public async Task<IActionResult> DeleteTable([FromHeader] string name)
        {
            try
            {
                var printers = await Repository.Tables.DeleteOneAsync(set => set.TableName == name);
                

                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }

        }

    }
}
