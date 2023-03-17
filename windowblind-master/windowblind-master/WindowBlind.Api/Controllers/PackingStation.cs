using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PrstringerProject;
using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
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


        private IRepository _repository;
        private IWebHostEnvironment _env;
        public static Dictionary<string, CCNumberRowCount> CBNumberCounter;

        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();

        public PackingStation(IRepository repository, IWebHostEnvironment env)
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

        public async Task CheckCBNumber()
        {
            CBNumberCounter = await GetCbNumberCountFromFile();
        }

        [HttpGet("GetReadyToPack")]
        public async Task<IActionResult> GetReadyToPack([FromHeader] string CbOrLineNumber)
        {
            try
            {
                //await CheckCBNumber();

                var PackingColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "PackingStation").Result.FirstOrDefaultAsync();
                var PackingColumns = PackingColumnsSetting.settingPath.Split("@@@").ToList();
                if (PackingColumns[PackingColumns.Count - 1] == "") PackingColumns.RemoveAt(PackingColumns.Count - 1);


                for (var i = 0; i < PackingColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(PackingColumns[i]))
                    {
                        PackingColumns[i] = ColumnMapper[PackingColumns[i]];
                    }
                }

                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "CB Number");
                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "Line No");

                FabricCutterCBDetailsModel data = new FabricCutterCBDetailsModel();

                // lets assume this was line number
                var FabricCutterOflogModels = await _repository.HoistStation.FindAsync(log => log.ProcessType == "Qualified" && log.status == "Qualified" && (log.CBNumber == CbOrLineNumber || log.LineNumber == CbOrLineNumber));
                var FabricCutterlistOflogModels = FabricCutterOflogModels.ToList();



                foreach (var row in FabricCutterlistOflogModels)
                {

                    row.row.rows_AssociatedIds.Clear();
                    row.row.Row["FromHoldingStation"] = "NO";

                    // row.row.Row["Total"] = CBNumberCounter[row.CBNumber].ToString();
                    row.row.UniqueId = row.Id;
                    row.row.rows_AssociatedIds.Add(row.Id);

                    var RowsWithTheSameCBNumber = _repository.PackingStation.Find(pack => pack.CBNumber == row.CBNumber && pack.status == "Packed").ToList();

                    if (RowsWithTheSameCBNumber == null)
                    {
                        row.row.Row["Packed"] = "0";
                    }
                    else
                    {
                        row.row.Row["Packed"] = RowsWithTheSameCBNumber.Count.ToString();
                    }

                    data.Rows.Add(row.row);

                }



                data.ColumnNames = PackingColumns;
                data.ColumnNames.Add("Status");
                data.ColumnNames.Insert(0, "Blind Number");

                return Ok(data);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }


        [HttpPost("pushLinesNoToPackingStation")]
        public async Task<IActionResult> pushLinesNoToPackingStation(CreateFileAndLabelModel model)
        {

            try
            {
                await CheckCBNumber();
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
                    log.status = "Packed";
                    log.ProcessType = row.PackingType;
                    log.TableName = model.tableName;
                    log.Message = "This Line No.: " + log.LineNumber + ", has been Qualified at " + log.dateTime;
                    await _repository.PackingStation.InsertOneAsync(log);
                    await _repository.Logs.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId,
                         Builders<LogModel>.Update.Set(p => p.status, "Packed"), new UpdateOptions { IsUpsert = false });
                    await _repository.HoistStation.UpdateManyAsync(log => log.LineNumber == row.Row["Line No"] && log.Id == row.UniqueId,
                        Builders<LogModel>.Update.Set(p => p.status, "Packed"), new UpdateOptions { IsUpsert = false });
                }
                var cbNumbersRowsInPackingDB = await _repository.PackingStation.FindAsync(log => log.CBNumber == model.data.Rows[0].Row["CB Number"] && log.status == "Packed").Result.ToListAsync();

                if (cbNumbersRowsInPackingDB != null)
                {
                    var count = cbNumbersRowsInPackingDB.Count;
                    if (count == int.Parse(cbNumbersRowsInPackingDB[0].row.Row["Total"]))
                    {
                        await _repository.PackingStation.UpdateManyAsync(log => log.CBNumber == cbNumbersRowsInPackingDB[0].CBNumber,
                      Builders<LogModel>.Update.Set(p => p.status, model.data.Rows[0].Row["Status"]), new UpdateOptions { IsUpsert = false });
                    }
                }


                return Ok(true);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        private async Task<Dictionary<string, CCNumberRowCount>> GetCbNumberCountFromFile()
        {
            Dictionary<string, CCNumberRowCount> CBNumberCounter = new Dictionary<string, CCNumberRowCount>();

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
                for (int i = start.Column; i <= end.Column; i++)
                {
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    if (Headertext == "W/Order NO")
                        for (int j = start.Row + 1; j <= end.Row; j++)
                        {
                            var text = worksheet.Cells[j, i].Text.Trim();

                            if (!CBNumberCounter.ContainsKey(text)) CBNumberCounter[text] = new CCNumberRowCount
                            {
                                Count = 0,
                                rows = new List<FabricCutterCBDetailsModelTableRow>()
                            };

                            CBNumberCounter[text].Count++;

                        }
                }
            }
            System.IO.File.Delete(ctbsodumpPath);
            return CBNumberCounter;

        }

        [HttpGet("GetHeldObjects")]
        public async Task<IActionResult> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "Packing" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {
                    Data.Rows.Add(item.Row);
                }

                var PackingColumnsSetting = await _repository.Settings.FindAsync(setting => setting.settingName == "SelectedColumnsNames" && setting.applicationSetting == "PackingStation").Result.FirstOrDefaultAsync();
                var PackingColumns = PackingColumnsSetting.settingPath.Split("@@@").ToList();
                if (PackingColumns[PackingColumns.Count - 1] == "") PackingColumns.RemoveAt(PackingColumns.Count - 1);

                for (var i = 0; i < PackingColumns.Count; i++)
                {

                    if (ColumnMapper.ContainsKey(PackingColumns[i]))
                    {
                        PackingColumns[i] = ColumnMapper[PackingColumns[i]];
                    }
                }

                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "CB Number");
                //PackingColumns = LogCut.AddColumnIfNotExists(PackingColumns, "Line No");
                if (Data.Rows.Count != 0)
                    Data.ColumnNames = PackingColumns;

                return Ok(Data);

            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpPost("ClearOrdersFromPacking")]
        public async Task<IActionResult> ClearOrdersFromPacking([FromBody] CreateFileAndLabelModel model)
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
                        await _repository.HoistStation.UpdateOneAsync(log => log.Id == id,
                                                                Builders<LogModel>.Update.Set(p => p.status, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });
                    }
                }
                return Ok(true);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }

        [HttpPost("PackingSend")]
        public async Task<IActionResult> PackingSend(CreateFileAndLabelModel model)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var LogCutOutputSettings = await _repository.Tables.FindAsync(e => e.TableName == model.tableName);
                var LogCutOutputPath = LogCutOutputSettings.FirstOrDefault().OutputPath;
                if (LogCutOutputPath == "") return Ok(false);
                DirectoryInfo f = new DirectoryInfo(LogCutOutputPath);

                if (!f.Exists) return Ok(false);

                if (model.printer == null || model.printer == "" || model.printer2nd == null || model.printer2nd == "-") return Ok(false);

                var data = model.data;
                var printerName = model.printer;
                var printerName2nd = model.printer2nd;
                var tableName = model.tableName;
                var strconcat = "";
                List<string> labels = new List<string>();
                var strRS232Width = "";
                foreach (var item in data.Rows)
                {
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });
                    if (item.Row["Department"] == "") continue;
                    var someoftotalprefix = item.Row["SomeOfTotal"].Trim();
                    someoftotalprefix = someoftotalprefix.Substring(0, someoftotalprefix.IndexOf(" "));
                    someoftotalprefix = new string('0', 4 - someoftotalprefix.Length) + someoftotalprefix;

                    var wordsOfAddress3 = item.Row["Address 3"].Split(" ");
                    var LastwordOfAddress3 = wordsOfAddress3[wordsOfAddress3.Length - 1];


                    strconcat = item.Row["Debtor Order Number"] + "/" + someoftotalprefix + "@" + item.Row["CB Number"];
                    strconcat += "@" + item.Row["Debtor Order Number"].Substring(0, item.Row["Debtor Order Number"].IndexOf(' ')) + "@" + item.Row["Order Department"] + "@" + "Viewscape Pty.Ltd";//item.Row["Supplier"];
                    strconcat += "@" + item.Row["Department"] + "@" + item.Row["Location"] + "@" + item.Row["Width"];
                    strconcat += "@" + item.Row["Drop"] + "@" + item.Row["Line No"] + "@" + item.Row["SomeOfTotal"];
                    strconcat += "@" + item.Row["Customer"] + "@" + item.Row["Carrier"] + " " + LastwordOfAddress3;
                    strconcat += "@" + item.Row["Address 1"];
                    strconcat += "@" + ((!String.IsNullOrEmpty(item.Row["Address 1"].Trim())) ? ", " : "") + item.Row["Address 2"];
                    strconcat += "@" + ((!String.IsNullOrEmpty(item.Row["Address 2"].Trim())) ? ", " : "") + item.Row["Address 3"];
                    strconcat += "@" + item.Row["Postcode"];
                    strconcat += "@" + item.Row["Description"];
                    var status = item.Row["Status"];
                    strconcat += status == "Dispatch" ? "@D" : status == "Holding bay" ? "@H" : "@D-H";

                    labels.Add(strconcat);
                    strRS232Width += item.Row["CutWidth"].ToString().Replace("mm", "");

                    bool res = await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), model.tableName, model.userName, System.DateTime.Now.ToString(), item.Row["Alpha"], "LogCut", item);

                }

                // the printing

                for (int k = 0; k < labels.Count; k++)
                {
                    var strParameterArray = labels[k].ToString().Split("@");

                    if (strParameterArray[11].Contains("Spotlight", StringComparison.OrdinalIgnoreCase))
                        PrintReport(printerName, strParameterArray, "PinkLabel.rdlc");

                    else
                        PrintReport(printerName2nd, strParameterArray, "LargeLabel.rdlc");


                }
                return Ok(true);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }


        }

        public async Task<bool> insertLog(string cbNumber, string barCode, string tableNo, string uName, string datetime, string item, string ProcessType, FabricCutterCBDetailsModelTableRow row)
        {

            LogModel log = new LogModel();
            log.UserName = uName;
            log.CBNumber = cbNumber;
            log.status = "IDLE";
            log.Id = row.UniqueId;
            foreach (var key in row.Row.Keys.ToList())
            {
                if (key == "")
                {
                    row.Row.Remove(key); continue;
                }
                var ind = key.IndexOf(".");
                if (ind == -1) continue;

                var newKey = key.Replace(".", "");
                var value = row.Row[key];

                row.Row[newKey] = value;
                row.Row.Remove(key);
            }
            log.row = row;
            log.LineNumber = barCode;
            log.Item = item;
            log.dateTime = datetime;
            log.Message = (cbNumber + " " + barCode + " " + tableNo + " " + uName + " " + datetime);
            log.ProcessType = ProcessType;
            log.TableName = tableNo;
            await _repository.Logs.InsertOneAsync(log);
            return true;

        }

        public string PrintReport(string strPrinterName, string[] strParameterArray, string StrReportPath)
        {

            string mimtype = "";
            int extension = 1;
            var path = Path.Combine("E:\\Webapp_input files", "Printer Driver", StrReportPath);
            //var path = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject", StrReportPath);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("us-ascii");

            LocalReport report = new LocalReport(path);

            for (int i = 0; i < strParameterArray.Length; i++)
            {
                while (strParameterArray[i].IndexOf("  ") != -1)
                    strParameterArray[i] = strParameterArray[i].Replace("  ", " ");
                if (String.IsNullOrEmpty(strParameterArray[i]))
                    strParameterArray[i] = " ";
            }

            if (StrReportPath == "PinkLabel.rdlc")
            {
                PinkLabel obj = new PinkLabel();
                obj.PO = "PO#: " + strParameterArray[0].ToString().Split(" ").Last();
                obj.CCNumber = strParameterArray[1].ToString();
                obj.Cust = "Cust: " + strParameterArray[2].ToString();
                obj.CustRef = "Cust Ref: " + strParameterArray[3].ToString();
                obj.Supplier = "Supplier: " + strParameterArray[4].ToString();
                obj.Department = strParameterArray[5].ToString();
                obj.Location = "Location: " + strParameterArray[6].ToString();
                obj.Width = "W: " + strParameterArray[7].ToString();
                obj.Drop = "D: " + strParameterArray[8].ToString();
                obj.LineNumber = strParameterArray[0].ToString().Split(" ").Last(); // strParameterArray[9].ToString();
                obj.SomeOfTotal = strParameterArray[10].ToString();
                obj.Customer = "Ship To: " + strParameterArray[11].ToString();
                obj.Carrier = strParameterArray[12].ToString();
                obj.Address1 = strParameterArray[13].ToString();
                obj.Address1 += strParameterArray[14].ToString() + strParameterArray[15].ToString();
                obj.PostCode = strParameterArray[16].ToString();
                obj.Status = strParameterArray[18].ToString();
                List<PinkLabel> ls = new List<PinkLabel> {
                    obj
                    };
                report.AddDataSource("PinklLabel", ls);

            }
            else
            {
                BigLabel obj = new BigLabel();
                obj.CBNumber = strParameterArray[1].ToString();
                obj.Carrier = strParameterArray[12].ToString();
                obj.Customer = strParameterArray[11].ToString();
                obj.PO = strParameterArray[0].ToString();
                obj.Description = strParameterArray[17].ToString();
                obj.Location = "Location: " + strParameterArray[6].ToString();
                obj.Width = "Width: " + strParameterArray[7].ToString() + "";
                obj.Drop = "Drop: " + strParameterArray[8].ToString() + "";
                obj.SomeOfTotal = "Quantity " + strParameterArray[10].ToString();
                obj.FittingAddress = strParameterArray[13].ToString();
                obj.Department = strParameterArray[5].ToString();
                obj.Status = strParameterArray[18].ToString();

                List<BigLabel> ls = new List<BigLabel> {
                    obj
                    };
                report.AddDataSource("BigLabel", ls);
            }


            byte[] result = report.Execute(RenderType.Pdf, extension, null, mimtype).MainStream;

            var outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "PackingStationPrintFiles", Guid.NewGuid().ToString() + ".pdf");
            //var //outputPath = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject\\Delete", Guid.NewGuid().ToString() + ".jpg");
            using (FileStream stream = new FileStream(outputPath, FileMode.Create))
            {
                stream.Write(result, 0, result.Length);
            }



            bool printedOK = true;
            string printErrorMessage = "";

            PdfDocument doc = new PdfDocument();

            doc.LoadFromFile(outputPath);
            doc.PrintSettings.PrinterName = strPrinterName;
            //SizeF size = doc.Pages[0].ActualSize;
            //PaperSize paper = new("Custom", (int)size.Width, (int)size.Height);
            //paper.RawKind = (int)PaperKind.Custom;
            //doc.PrintSettings.PaperSize = paper;
            //doc.SaveToFile(outputPath, 1, 1, FileFormat.SVG);
            //doc.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize);
            doc.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize, false);
            doc.PrintSettings.SetPaperMargins(0, 0, 0, 0);
            doc.PrintSettings.SelectPageRange(1, 1);
            doc.PrintSettings.Landscape = false;
            //doc.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.ActualSize);
            doc.Print();

            return "";

        }


    }
}
