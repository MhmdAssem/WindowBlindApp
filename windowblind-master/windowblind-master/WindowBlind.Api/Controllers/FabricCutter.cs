using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FabricCutter : ControllerBase
    {

        public FabricCutter(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;

        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails([FromHeader] string CBNumber)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                #region init

                Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();
                ColumnMapper.Add("Customer Name 1", "Customer");
                ColumnMapper.Add("W/Order NO", "CB Number");
                ColumnMapper.Add("Qty", "Quantity");
                ColumnMapper.Add("Width", "Measured Width");
                ColumnMapper.Add("Drop", "Measured Drop");
                ColumnMapper.Add("Fabric", "Fabric Type");
                ColumnMapper.Add("Colour", "Fabric Colour");
                ColumnMapper.Add("Pull Type / Control Type /Draw Type", "Control Type");

                #endregion



                #region Reading the config
                /// get the dumb file Path
                var ctbsodumpSetting = await _repository.Settings.FindAsync(e => e.settingName == "ctbsodump");
                var ctbsodumpPath = ctbsodumpSetting.FirstOrDefault().settingPath;

                /// get the sheet name
                var SheetNameSetting = await _repository.Settings.FindAsync(e => e.settingName == "SheetName");
                var SheetNamePath = SheetNameSetting.FirstOrDefault().settingPath;

                /// get the SelectedColumns
                var SelectedColumnsSetting = await _repository.Settings.FindAsync(e => e.settingName == "SelectedColumnsNames");
                var SelectedColumnsPath = SelectedColumnsSetting.FirstOrDefault().settingPath.Split("@@@").ToList();

                if (SelectedColumnsPath.Count == 1 && SelectedColumnsPath[0].Trim() == "") SelectedColumnsPath = new List<string>();

                /// get Fabric Roll WIdth File

                var FBRSetting = await _repository.Settings.FindAsync(e => e.settingName == "Fabric Rollwidth");
                var FBRPath = FBRSetting.FirstOrDefault().settingPath;
                /// get deduct width file
                var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "Deduction");
                var DeductionPath = DeductionSetting.FirstOrDefault().settingPath;
                ///get Lathe file path
                var LatheSetting = await _repository.Settings.FindAsync(e => e.settingName == "PVCLathe Fabric");
                var LathePath = LatheSetting.FirstOrDefault().settingPath;

                #endregion

                #region Checking The Paths

                FileInfo file = new FileInfo(ctbsodumpPath);
                if (!file.Exists) return new JsonResult(false);


                file = new FileInfo(FBRPath);
                if (!file.Exists) return new JsonResult(false);


                file = new FileInfo(DeductionPath);
                if (!file.Exists) return new JsonResult(false);


                file = new FileInfo(LathePath);
                if (!file.Exists) return new JsonResult(false);

                #endregion

                #region Reading Data
                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
                file = new FileInfo(ctbsodumpPath);
                if (!file.Exists) return null;
                List<string> names = new List<string>();
                //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
                int generalBlindNumber = 1;
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.Where(e => e.Name == SheetNamePath).FirstOrDefault();
                    if (worksheet == null) return null;
                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    bool gotColumns = (SelectedColumnsPath.Count > 0) ? true : false;

                    Dictionary<int, int> indexToRemove = new Dictionary<int, int>();
                    for (int i = start.Column; i < end.Column; i++)
                    {
                        var text = worksheet.Cells[2, i].Text.Trim();
                        if (text.StartsWith("CB"))
                        {
                            for (int j = start.Row + 1; j < end.Row; j++)
                                if (worksheet.Cells[j, i].Text.Trim() != CBNumber) indexToRemove[j] = 1;

                            break;
                        }


                    }



                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        if (indexToRemove.ContainsKey(i)) continue;
                        Dictionary<string, string> row = new Dictionary<string, string>();
                        int RowQty = 0;
                        for (int j = start.Column; j < end.Column; j++)
                        {
                            var Headertext = worksheet.Cells[1, j].Text.Trim();
                            if (Headertext.Contains("Qty"))
                            {
                                RowQty = int.Parse(worksheet.Cells[i, j].Text.Trim());
                            }




                            var cell = worksheet.Cells[i, j].Text.Trim();

                            if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];
                            row[Headertext] = cell;
                            if (!Data.ColumnNames.Contains(Headertext) && SelectedColumnsPath.Contains(worksheet.Cells[1, j].Text.Trim()))
                                Data.ColumnNames.Add(Headertext);
                        }
                        FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                        for (int cntr = generalBlindNumber; cntr < RowQty + generalBlindNumber; cntr++)
                        {
                            TblRow.BlindNumbers.Add(cntr);
                        }
                        generalBlindNumber += RowQty;
                        TblRow.Row = row;
                        TblRow.UniqueId = Guid.NewGuid().ToString();
                        Data.Rows.Add(TblRow);
                    }


                    package.Dispose();
                }


                Dictionary<string, string> FabricRollwidth = new Dictionary<string, string>();
                Dictionary<string, int> ControlTypevalues = new Dictionary<string, int>();
                Dictionary<string, List<string>> LatheType = new Dictionary<string, List<string>>();

                file = new FileInfo(FBRPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        FabricRollwidth[worksheet.Cells[i, 1].Text.Trim()] = worksheet.Cells[i, 2].Text.Trim();
                    }
                }

                file = new FileInfo(DeductionPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        ControlTypevalues[worksheet.Cells[i, 1].Text.Trim()] = int.Parse(worksheet.Cells[i, 2].Text.Trim());
                    }
                }

                file = new FileInfo(LathePath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        if (!LatheType.ContainsKey(worksheet.Cells[i, 1].Text.Trim()))
                            LatheType[worksheet.Cells[i, 1].Text.Trim()] = new List<string>();

                        LatheType[worksheet.Cells[i, 1].Text.Trim()].Add(worksheet.Cells[i, 2].Text.Trim());

                    }
                }

                #endregion


                #region Customization of columns

                var Bindcntr = -1;
                foreach (var item in Data.Rows)
                {
                    Bindcntr++;
                    item.Row["Total Blinds"] = (generalBlindNumber - 1).ToString();
                    if (item.Row["Bind. Type/# Panels/Rope/Operation"].ToString().TrimEnd() != "")
                    {
                        if (ControlTypevalues.ContainsKey(item.Row["Bind. Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()))
                        {
                            item.Row["Cut Width"] = (int.Parse(item.Row["Measured Width"]) + ControlTypevalues[item.Row["Bind. Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()]).ToString();
                        }
                    }
                    else
                    {
                        var val = item.Row["Measured Width"] == "" ? "0" : item.Row["Measured Width"];
                        item.Row["Cut Width"] = (Convert.ToInt32(val) - 30).ToString();
                    }


                    if (item.Row["Description"].TrimEnd().EndsWith("FIN 36") && item.Row["Bind. Type/# Panels/Rope/Operation"] == "Motorised")
                    {
                        item.Row["Measured Width"] = (Convert.ToInt32(item.Row["Measured Width"]) - 5).ToString();
                    }


                    if (item.Row["Fabric Type"] != "")
                    {
                        var fab = item.Row["Fabric Type"].TrimEnd();
                        if (FabricRollwidth.ContainsKey(fab.Substring(0, fab.LastIndexOf(' ')).TrimEnd()))
                        {
                            item.Row["Roll Width"] = FabricRollwidth[fab.Substring(0, fab.IndexOf(' ')).TrimEnd()].ToLower();
                        }
                        else
                        {
                            item.Row["Roll Width"] = fab.Substring(fab.LastIndexOf(' ') + 1).Trim().ToLower();
                        }
                        item.Row["Roll Width"].Replace("mm", "");
                    }


                    item.Row["Trim Type"] = item.Row["Description"].Substring(item.Row["Description"].ToString().TrimEnd().Length - 6);



                    item.Row["Track Colour"] = item.Row["Track Col/Roll Type/Batten Col"].ToString().TrimEnd();



                    if (item.Row["Pull Colour/Bottom Weight/Wand Len"].ToString().TrimEnd() != "")
                        item.Row["Pull Colour"] = item.Row["Pull Colour/Bottom Weight/Wand Len"].ToString().TrimEnd();
                    else
                    {
                        string LookupKey = item.Row["Fabric Type"].ToString().TrimEnd();

                        if (LatheType.ContainsKey(LookupKey) && LatheType[LookupKey].Contains(item.Row["Fabric Colour"].ToString().TrimEnd()))
                        {
                            item.Row["Pull Colour"] = "PVC Lathe";
                        }
                        else
                        {
                            item.Row["Pull Colour"] = "Lathe";
                        }
                    }
                    item.Row["Barcode"] = item.Row["Line No."].ToString().TrimEnd();


                    if (item.Row["Cntrl Side"].ToString().TrimEnd() != "")
                    {
                        item.Row["Control Side"] = item.Row["Cntrl Side"].ToString().TrimEnd().ToCharArray()[0].ToString();// Added on 8-10-2016
                    }
                    else
                    {
                        item.Row["Control Side"] = "N";

                    }

                }
                Data.ColumnNames.Add("Roll Width");
                #endregion
                return new JsonResult(Data);

            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }
        }

        public void PrintReport(string strNoCopy, string strPrinterName, string[] strParameterArray)
        {

            try
            {
                string mimtype = "";
                int extension = 1;
                var path = Path.Combine(_env.ContentRootPath, "Printer Driver", "FabricCutter.rdlc");
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("cbNumber", strParameterArray[0]);
                parameters.Add("width", strParameterArray[1] + " mm");
                parameters.Add("drop", strParameterArray[2] + " mm");
                parameters.Add("customer", strParameterArray[3].ToString());
                parameters.Add("department", strParameterArray[4].ToString());
                parameters.Add("type", strParameterArray[5].ToString());
                parameters.Add("fabric", strParameterArray[6].ToString());
                parameters.Add("color", strParameterArray[7].ToString());
                parameters.Add("controltype", strParameterArray[8].ToString());
                parameters.Add("lathe", strParameterArray[9].ToString());
                parameters.Add("char", strParameterArray[10].Split(" ")[0].ToString());
                parameters.Add("cutwidth", strParameterArray[10].Split(" ")[1].ToString());
                parameters.Add("lineNumber", strParameterArray[11].ToString());

                parameters.Add("controlside", strParameterArray[12].Split(" ")[0].ToString());
                parameters.Add("someoftotal", strParameterArray[12].Split(" ")[1].ToString() + " of " + strParameterArray[13].ToString());

                LocalReport localReport = new LocalReport(path);
                var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimtype);
                var outputPath = Path.Combine(_env.ContentRootPath, "Printer Driver", "FabricCutterPrintFiles", Guid.NewGuid().ToString() + ".pdf");
                using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                {
                    stream.Write(result.MainStream, 0, result.MainStream.Length);
                }



                bool printedOK = true;
                string printErrorMessage = "";
                try
                {
                    PdfDocument pdf = new PdfDocument(outputPath);
                    pdf.PrintSettings.PrinterName = strPrinterName;
                    //pdf.PrintSettings.PaperSize = new System.Drawing.Printing.PaperSize("Custom", _labelWidth, _labelHeight);
                    pdf.PrintSettings.SetPaperMargins(1, 1, 1, 1);
                    pdf.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.ActualSize);
                    //pdf.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize, true);
                    pdf.Print();

                }
                catch (Exception ex)
                {
                    printErrorMessage = "Printing Error: " + ex.ToString();
                    printedOK = false;
                }

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void PrintLabels(string strParameter)
        {
            string[] strpara = null;
            strpara = strParameter.Replace("|@", "|").Split('|');

            if (strpara.GetUpperBound(0) == 0)
            {
                System.Environment.Exit(0);
            }
            else
            {

                string strPrinterName = null;
                strPrinterName = strpara[0];

                for (int x = 1; x <= strpara.Length - 1; x++)
                {

                    string[] strParameterArray = strpara[x].Split('@');

                    if (strParameterArray.GetUpperBound(0) == 0)
                    {
                        System.Environment.Exit(0);
                    }
                    else
                    {
                        PrintReport("1", strPrinterName, strParameterArray);
                    }
                }

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
                await _repository.Logs.InsertOneAsync(log);
                return true;
            }
            catch (Exception e)
            {

                return false;
            }

        }

        public void writefile(string data, string filename, string file)
        {
            filename = Path.Combine(filename, "FabricCutOutput_" + DateTime.Now.ToString("dd-mm-yyyy  hh-mm-ss") + ".txt");
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(data);
            }
        }

        private string CalculateAlphabeticFromNumber(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        [HttpPost("CreateFilesAndLabels")]
        public async Task<IActionResult> CreateFilesAndLabels([FromBody] CreateFileAndLabelModel model)
        {
            var Data = model.data;
            var TableNumber = model.tableName;
            var UserName = model.userName;
            var PrinterName = model.printer;
            var FBRSetting = await _repository.Settings.FindAsync(e => e.settingName == "Fabric Cutter Output");
            var FBRPath = FBRSetting.FirstOrDefault().settingPath;

            if (FBRPath == "") return new JsonResult(false);
            DirectoryInfo F = new DirectoryInfo(FBRPath);
            if (!F.Exists) return new JsonResult(false);


            StringBuilder sb = new StringBuilder();
            StringBuilder labels = new StringBuilder();
            foreach (var item in Data.Rows)
            {

                sb.Append("START##\t" + item.Row["Department"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append("ORDER NUMBER\t" + item.Row["CB Number"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append("LINE NUMBER\t" + item.Row["Blind Number"].ToString().TrimEnd() + "\t\t" + Environment.NewLine);
                sb.Append("QUANTITY\t" + item.Row["Quantity"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append("MEASURED WIDTH\t" + item.Row["Measured Width"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append("MEASURED DROP\t" + item.Row["Measured Drop"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("CONTROL TYPE\t" + item.Row["Control Type"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("HEM\t" + Environment.NewLine);
                sb.Append("FABRIC TYPE\t" + item.Row["Roll Width"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("FABRIC COLOUR\t" + item.Row["Fabric Colour"].ToString().TrimEnd() + Environment.NewLine);
                sb.Append("TRIM TYPE\t" + item.Row["Trim Type"].ToString().TrimEnd() + Environment.NewLine);
                item.Row["Alpha Number"] = CalculateAlphabeticFromNumber(int.Parse(item.Row["Blind Number"]));
                bool res = await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), TableNumber, UserName, System.DateTime.Now.ToShortDateString(), item.Row["Alpha Number"], "FabricCut", item);


                labels.Append("@" + item.Row["CB Number"].ToString().TrimEnd());

                if (item.Row["Trim Type"].ToString().TrimEnd() == "FIN 36" && item.Row["Control Type"].ToString().TrimEnd() == "Motorised")
                {
                    labels.Append("@" + (Convert.ToInt32((item.Row["Measured Width"].ToString().TrimEnd() == "") ? "0" : item.Row["Measured Width"].ToString().TrimEnd()) + 5).ToString());
                }
                else
                {
                    labels.Append("@" + item.Row["Measured Width"].ToString().TrimEnd());
                }

                labels.Append("@" + item.Row["Measured Drop"].ToString().TrimEnd());
                labels.Append("@" + item.Row["Customer"].ToString().TrimEnd());
                labels.Append("@" + item.Row["Department"].ToString().TrimEnd());


                if (item.Row["Track Colour"].ToString().TrimEnd() == "OVEROLL")
                {
                    labels.Append("@O/R");
                }
                else
                {
                    labels.Append("@" + item.Row["Track Colour"].ToString().TrimEnd());
                }

                item.Row["Alpha Number"] = CalculateAlphabeticFromNumber(int.Parse(item.Row["Blind Number"]));
                if (item.Row["Fabric Type"] != "")
                {
                    labels.Append("@" + item.Row["Fabric Type"].ToString().TrimEnd().Substring(0, item.Row["Fabric Type"].ToString().TrimEnd().Length - 6).TrimEnd());
                }
                else
                {
                    labels.Append("@");
                }
                labels.Append("@" + item.Row["Fabric Colour"].ToString().TrimEnd());
                labels.Append("@" + item.Row["Trim Type"].ToString().TrimEnd());
                labels.Append("@" + item.Row["Pull Colour"].ToString().TrimEnd());//Added on 8-10-2016
                labels.Append("@" + item.Row["Alpha Number"].ToString().TrimEnd() + "   " + item.Row["Cut Width"].ToString().TrimEnd());
                labels.Append("@" + item.Row["Barcode"].ToString().TrimEnd()); /// it's the line number
                labels.Append("@" + item.Row["Control Side"].ToString().TrimEnd() + " " + item.Row["Blind Number"].ToString().TrimEnd()); //TODO: Suppy cut width here
                labels.Append("@" + item.Row["Total Blinds"].ToString().TrimEnd());
                labels.Append("|");


            }


            string labeldata = labels.ToString().TrimEnd('|');

            writefile(sb.ToString().TrimEnd(), FBRPath, "");



            PrintLabels(PrinterName + "|" + labeldata);

            return new JsonResult(true);
        }

        [HttpPost("PrintLabelsOnly")]
        public IActionResult PrintLabelsOnly([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                var Data = model.data;
                var TableNumber = model.tableName;
                var PrinterName = model.printer;
                var UserName = model.userName;

                StringBuilder labels = new StringBuilder();
                foreach (var item in Data.Rows)
                {

                    labels.Append("@" + item.Row["CB Number"].ToString().TrimEnd());

                    if (item.Row["Trim Type"].ToString().TrimEnd() == "FIN 36" && item.Row["Control Type"].ToString().TrimEnd() == "Motorised")
                    {
                        labels.Append("@" + (Convert.ToInt32((item.Row["Measured Width"].ToString().TrimEnd() == "") ? "0" : item.Row["Measured Width"].ToString().TrimEnd()) + 5).ToString());
                    }
                    else
                    {
                        labels.Append("@" + item.Row["Measured Width"].ToString().TrimEnd());
                    }

                    labels.Append("@" + item.Row["Measured Drop"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["Customer"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["Department"].ToString().TrimEnd());


                    if (item.Row["Track Colour"].ToString().TrimEnd() == "OVEROLL")
                    {
                        labels.Append("@O/R");
                    }
                    else
                    {
                        labels.Append("@" + item.Row["Track Colour"].ToString().TrimEnd());
                    }
                    item.Row["Alpha Number"] = CalculateAlphabeticFromNumber(int.Parse(item.Row["Blind Number"]));
                    if (item.Row["Fabric Type"] != "")
                    {
                        labels.Append("@" + item.Row["Fabric Type"].ToString().TrimEnd().Substring(0, item.Row["Fabric Type"].ToString().TrimEnd().Length - 6).TrimEnd());
                    }
                    else
                    {
                        labels.Append("@");
                    }
                    labels.Append("@" + item.Row["Fabric Colour"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["Trim Type"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["Pull Colour"].ToString().TrimEnd());//Added on 8-10-2016
                    labels.Append("@" + item.Row["Alpha Number"].ToString().TrimEnd() + "   " + item.Row["Cut Width"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["Barcode"].ToString().TrimEnd()); /// it's the line number
                    labels.Append("@" + item.Row["Control Side"].ToString().TrimEnd() + " " + item.Row["Blind Number"].ToString().TrimEnd()); //TODO: Suppy cut width here
                    labels.Append("@" + item.Row["Total Blinds"].ToString().TrimEnd());
                    labels.Append("|");
                }


                string labeldata = labels.ToString().TrimEnd('|');

                PrintLabels(PrinterName + "|" + labeldata);
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }
    }
}
