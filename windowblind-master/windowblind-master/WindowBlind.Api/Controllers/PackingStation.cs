using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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


                return true;
            }
            catch (Exception)
            {

                return false;
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

                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
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
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.Message);
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
                if (LogCutOutputPath == "") return new JsonResult(false);
                DirectoryInfo f = new DirectoryInfo(LogCutOutputPath);

                if (!f.Exists) return new JsonResult(false);

                if (model.printer == null || model.printer == "" || model.printer2nd == null || model.printer2nd == "-") return new JsonResult(false);

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
                    strconcat = item.Row["Debtor Order Number"] + "@" + item.Row["CB Number"];
                    strconcat += "@" + item.Row["Debtor Order Number"].Substring(0, item.Row["Debtor Order Number"].IndexOf(' ')) + "@" + item.Row["Order Department"] + "@" + item.Row["Supplier"];
                    strconcat += "@" + item.Row["Department"] + "@" + item.Row["Location"] + "@" + item.Row["Width"];
                    strconcat += "@" + item.Row["Drop"] + "@" + item.Row["Line No"] + "@" + item.Row["SomeOfTotal"];
                    strconcat += "@" + item.Row["Customer"] + "@" + item.Row["Carrier"];
                    strconcat += "@" + item.Row["Address 1"];
                    strconcat += "@" + item.Row["Address 2"];
                    strconcat += "@" + item.Row["Address 3"];
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

                    if (strParameterArray[11] == "Spotlight Sandown")
                        PrintReport(printerName, strParameterArray, "PinkLabel.rdlc");

                    else
                        PrintReport(printerName2nd, strParameterArray, "LargeLabel.rdlc");


                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }


        }

        public async Task<bool> insertLog(string cbNumber, string barCode, string tableNo, string uName, string datetime, string item, string ProcessType, FabricCutterCBDetailsModelTableRow row)
        {
            try
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
            catch (Exception)
            {

                return false;
            }

        }

        public string PrintReport(string strPrinterName, string[] strParameterArray, string StrReportPath)
        {

            try
            {
                string mimtype = "";
                int extension = 1;
                var path = Path.Combine("E:\\Webapp_input files", "Printer Driver", StrReportPath);
                //var path = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject", StrReportPath);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("us-ascii");
                var parametersList = new Dictionary<string, string>();


                for (int i = 0; i < strParameterArray.Length; i++)
                {
                    while (strParameterArray[i].IndexOf("  ") != -1)
                        strParameterArray[i] = strParameterArray[i].Replace("  ", " ");
                    if (String.IsNullOrEmpty(strParameterArray[i]))
                        strParameterArray[i] = " ";
                }

                if (StrReportPath == "PinkLabel.rdlc")
                {
                    parametersList.Add("PO", strParameterArray[0].ToString());
                    parametersList.Add("CCNumber", strParameterArray[1].ToString());
                    parametersList.Add("Cust", "Cust: " + strParameterArray[2].ToString());
                    parametersList.Add("CustRef", "Cust Ref: " + strParameterArray[3].ToString());
                    parametersList.Add("Supplier", "Supplier: " + strParameterArray[4].ToString());
                    parametersList.Add("Department", strParameterArray[5].ToString());
                    parametersList.Add("Location", "Location: " + strParameterArray[6].ToString());
                    parametersList.Add("Width", "W: " + strParameterArray[7].ToString());
                    parametersList.Add("Drop", "D: " + strParameterArray[8].ToString());
                    parametersList.Add("LineNumber", strParameterArray[9].ToString());
                    parametersList.Add("SomeOfTotal", strParameterArray[10].ToString());
                    parametersList.Add("Customer", "Ship To: " + strParameterArray[11].ToString());
                    parametersList.Add("Carrier", strParameterArray[12].ToString());
                    parametersList.Add("Address1", strParameterArray[13].ToString());
                    parametersList.Add("Address2", strParameterArray[14].ToString());
                    parametersList.Add("Address3", strParameterArray[15].ToString());
                    parametersList.Add("PostCode", strParameterArray[16].ToString());
                    parametersList.Add("Status", strParameterArray[18].ToString());

                }
                else
                {
                    parametersList.Add("CBNumber", strParameterArray[1].ToString());
                    parametersList.Add("Carrier", strParameterArray[12].ToString());
                    parametersList.Add("Customer", strParameterArray[11].ToString());
                    parametersList.Add("PO", strParameterArray[0].ToString());
                    parametersList.Add("Description", strParameterArray[17].ToString());
                    parametersList.Add("Location", "Location: " + strParameterArray[6].ToString());
                    parametersList.Add("Width", "Width: " + strParameterArray[7].ToString() + "");
                    parametersList.Add("Drop", "Drop: " + strParameterArray[8].ToString() + "");
                    parametersList.Add("SomeOfTotal", "Quantity " + strParameterArray[10].ToString());
                    parametersList.Add("FittingAddress", strParameterArray[13].ToString());
                    parametersList.Add("Department", strParameterArray[5].ToString());
                    parametersList.Add("Status", strParameterArray[18].ToString());

                }





                LocalReport report = new LocalReport(path);


                byte[] result = report.Execute(RenderType.Image, extension, parametersList, mimtype).MainStream;

                var outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "PackingStationPrintFiles", Guid.NewGuid().ToString() + ".jpg");
                //var outputPath = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject\\Delete", Guid.NewGuid().ToString() + ".jpg");
                using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                {
                    stream.Write(result, 0, result.Length);
                }

                #region Converting Byte Array to Asci Encodeed Byte array
                Encoding ascii = Encoding.ASCII;

                using (StreamReader sr = new StreamReader(path, true))
                {
                    Encoding encoding = sr.CurrentEncoding;
                    byte[] asciiBytes = Encoding.Convert(encoding, ascii, result);
                    outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "PackingStationPrintFiles", "Encoded_" + Guid.NewGuid().ToString() + ".jpg");

                    using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                    {
                        stream.Write(asciiBytes, 0, asciiBytes.Length);
                    }
                }
                #endregion


                bool printedOK = true;
                string printErrorMessage = "";
                try
                {
                    PrintDocument pd = new PrintDocument();
                    PrintController printController = new StandardPrintController();
                    pd.PrintController = printController;
                    pd.PrinterSettings.PrinterName = strPrinterName;
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrinterSettings.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pd.PrintPage += (sndr, args) =>
                    {
                        System.Drawing.Image i = System.Drawing.Image.FromFile(outputPath);
                        System.Drawing.Rectangle m = args.MarginBounds;

                        //Logic below maintains Aspect Ratio 
                        if ((double)i.Width / (double)i.Height > (double)m.Width / (double)m.Height) // image is wider
                        {
                            m.Height = (int)((double)i.Height / (double)i.Width * (double)m.Width);
                        }
                        else
                        {
                            m.Width = (int)((double)i.Width / (double)i.Height * (double)m.Height);
                        }

                        //Calculating optimal orientation.
                        pd.DefaultPageSettings.Landscape = m.Height > m.Width;

                        // Putting image in center of page.
                        m.Y = 0;// (int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Height - m.Height) / 2);
                        m.X = 0;// (int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Width - m.Width) / 2);
                        args.Graphics.DrawImage(i, m);
                    };
                    pd.Print();
                }
                catch (Exception ex)
                {
                    printErrorMessage = "Printing Error: " + ex.ToString();
                    printedOK = false;
                    return ex.StackTrace;
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.StackTrace;
            }
        }


    }
}
