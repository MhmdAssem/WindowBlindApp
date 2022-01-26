﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
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
    public class FabricCutter : ControllerBase
    {

        public FabricCutter(IRepository repository, IWebHostEnvironment env)
        {
            path = "";
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private string path;
        private IWebHostEnvironment _env;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();
        string ctbsodumpPath, SheetNamePath, FBRPath, DeductionPath, LathePath;
        List<string> SelectedColumnsPath;
        Dictionary<string, string> FabricRollwidth;
        Dictionary<string, int> ControlTypevalues;
        Dictionary<string, List<string>> LatheType;
        Dictionary<string, int> ColumnsIndex;
        bool CBSearchOrLineNumberSearch;
        string LineNumber;
        int generalBlindNumber;
        private void Init()
        {
            if (ColumnMapper.Count > 0) return;
            ColumnMapper.Add("Customer Name 1", "Customer");
            ColumnMapper.Add("W/Order NO", "CB Number");
            ColumnMapper.Add("Qty", "Quantity");
            ColumnMapper.Add("Width", "Measured Width");
            ColumnMapper.Add("Drop", "Measured Drop");
            ColumnMapper.Add("Fabric", "Fabric Type");
            ColumnMapper.Add("Colour", "Fabric Colour");
            ColumnMapper.Add("Pull Type / Control Type /Draw Type", "Control Type");
            ColumnsIndex = new Dictionary<string, int>();
        }
        private async Task ReadConfig()
        {
            /// get the dumb file Path
            var ctbsodumpSetting = await _repository.Settings.FindAsync(e => e.settingName == "ctbsodump");
            ctbsodumpPath = ctbsodumpSetting.FirstOrDefault().settingPath;

            /// get the sheet name
            var SheetNameSetting = await _repository.Settings.FindAsync(e => e.settingName == "SheetName");
            SheetNamePath = SheetNameSetting.FirstOrDefault().settingPath;

            /// get the SelectedColumns
            var SelectedColumnsSetting = await _repository.Settings.FindAsync(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "FabricCutter");
            SelectedColumnsPath = SelectedColumnsSetting.FirstOrDefault().settingPath.Split("@@@").ToList();

            if (SelectedColumnsPath.Count == 1 && SelectedColumnsPath[0].Trim() == "") SelectedColumnsPath = new List<string>();

            /// get Fabric Roll WIdth File

            var FBRSetting = await _repository.Settings.FindAsync(e => e.settingName == "Fabric Rollwidth");
            FBRPath = FBRSetting.FirstOrDefault().settingPath;
            /// get deduct width file
            var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "Deduction");
            DeductionPath = DeductionSetting.FirstOrDefault().settingPath;
            ///get Lathe file path
            var LatheSetting = await _repository.Settings.FindAsync(e => e.settingName == "PVCLathe Fabric");
            LathePath = LatheSetting.FirstOrDefault().settingPath;

        }

        private bool CheckingPaths()
        {

            FileInfo file = new FileInfo(ctbsodumpPath);
            if (!file.Exists) return false;


            file = new FileInfo(FBRPath);
            if (!file.Exists) return false;


            file = new FileInfo(DeductionPath);
            if (!file.Exists) return false;


            file = new FileInfo(LathePath);
            if (!file.Exists) return false;

            return true;
        }

        private FabricCutterCBDetailsModel ReadingData(string CBNumber)
        {
            try
            {
                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
                var TempctbsodumpPath = CreateNewFile(ctbsodumpPath, ctbsodumpPath.Substring(0, ctbsodumpPath.IndexOf(".")) + Guid.NewGuid().ToString() + ctbsodumpPath.Substring(ctbsodumpPath.IndexOf(".")));
                FileInfo file = new FileInfo(TempctbsodumpPath);
                if (!file.Exists) return null;
                List<string> names = new List<string>();
                //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
                generalBlindNumber = 1;


                CBSearchOrLineNumberSearch = !char.IsDigit(CBNumber[0]);

                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.Where(e => e.Name == SheetNamePath).FirstOrDefault();
                    if (worksheet == null) return null;
                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    bool gotColumns = (SelectedColumnsPath.Count > 0) ? true : false;

                    Dictionary<int, int> indexToRemove = new Dictionary<int, int>();


                    if (!CBSearchOrLineNumberSearch)
                    {
                        var CBINdex = 0;
                        LineNumber = CBNumber;
                        for (int i = start.Column; i < end.Column; i++)
                        {
                            var text = worksheet.Cells[1, i].Text.Trim();

                            if (text.Equals("W/Order NO")) CBINdex = i;
                            if (text.Equals("Line No."))
                            {
                                for (int j = start.Row + 1; j < end.Row; j++)
                                    if (worksheet.Cells[j, i].Text.Trim() == LineNumber) { CBNumber = worksheet.Cells[j, CBINdex].Text.Trim(); break; }
                            }
                        }
                    }

                    for (int i = start.Column; i < end.Column; i++)
                    {
                        var Headertext = worksheet.Cells[1, i].Text.Trim();
                        ColumnsIndex[worksheet.Cells[1, i].Text.Trim()] = i;

                        if (Headertext.StartsWith("W/Order NO"))
                        {
                            for (int j = start.Row + 1; j < end.Row; j++)
                                if (worksheet.Cells[j, i].Text.Trim() != CBNumber) indexToRemove[j] = 1;
                        }
                    }

                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        if (indexToRemove.ContainsKey(i)) continue;
                        Dictionary<string, string> row = new Dictionary<string, string>();
                        row["Line No"] = "";
                        row["Customer"] = "";
                        row["Bind Type/# Panels/Rope/Operation"] = "";
                        row["Description"] = "";
                        row["Track Col/Roll Type/Batten Col"] = "";
                        row["Cntrl Side"] = "";
                        row["Control Type"] = "";
                        row["Pull Colour/Bottom Weight/Wand Len"] = "";

                        int RowQty = 0;
                        for (int j = start.Column; j < end.Column; j++)
                        {
                            var Headertext = worksheet.Cells[1, j].Text.Trim();
                            if (String.IsNullOrEmpty(Headertext)) continue;
                            Headertext = Headertext.Replace(".", "");

                            if (Headertext.Contains("Qty"))
                            {
                                RowQty = int.Parse(worksheet.Cells[i, j].Text.Trim());
                            }

                            if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "") { RowQty = 0; break; }


                            var cell = worksheet.Cells[i, j].Text.Trim();

                            if (ColumnMapper.ContainsKey(Headertext))
                                Headertext = ColumnMapper[Headertext];
                            row[Headertext] = cell;


                        }
                        if (RowQty == 0) continue;
                        FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                        for (int cntr = generalBlindNumber; cntr < RowQty + generalBlindNumber; cntr++)
                        {
                            TblRow.BlindNumbers.Add(cntr);
                        }
                        generalBlindNumber += RowQty;
                        TblRow.Row = row;
                        TblRow.UniqueId = Guid.NewGuid().ToString();
                        TblRow.rows_AssociatedIds.Add(TblRow.UniqueId);
                        Data.Rows.Add(TblRow);
                    }


                    package.Dispose();
                }

                System.IO.File.Delete(TempctbsodumpPath);

                FabricRollwidth = new Dictionary<string, string>();
                ControlTypevalues = new Dictionary<string, int>();
                LatheType = new Dictionary<string, List<string>>();

                var TempFBRPath = CreateNewFile(FBRPath, FBRPath.Substring(0, FBRPath.IndexOf(".")) + Guid.NewGuid().ToString() + FBRPath.Substring(FBRPath.IndexOf(".")));
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

                System.IO.File.Delete(TempFBRPath);


                var TempDeductionPath = CreateNewFile(DeductionPath, DeductionPath.Substring(0, DeductionPath.IndexOf(".")) + Guid.NewGuid().ToString() + DeductionPath.Substring(DeductionPath.IndexOf(".")));

                file = new FileInfo(TempDeductionPath);
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

                System.IO.File.Delete(TempDeductionPath);

                var TempLathePath = CreateNewFile(LathePath, LathePath.Substring(0, LathePath.IndexOf(".")) + Guid.NewGuid().ToString() + LathePath.Substring(LathePath.IndexOf(".")));

                file = new FileInfo(TempLathePath);
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

                System.IO.File.Delete(TempLathePath);


                return Data;
            }
            catch (Exception e)
            {

                throw;
            }

        }

        private void FabricCutProcess(ref FabricCutterCBDetailsModel Data)
        {
            var Bindcntr = -1;
            foreach (var item in Data.Rows)
            {
                Bindcntr++;
                item.Row["Total Blinds"] = (generalBlindNumber - 1).ToString();
                if (item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd() != "")
                {
                    if (ControlTypevalues.ContainsKey(item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()))
                    {
                        item.Row["Cut Width"] = (int.Parse(item.Row["Measured Width"]) + ControlTypevalues[item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()]).ToString();
                    }
                }
                else
                {
                    var val = item.Row["Measured Width"] == "" ? "0" : item.Row["Measured Width"];
                    item.Row["Cut Width"] = (Convert.ToInt32(val) - 30).ToString();
                }


                if (item.Row["Description"].TrimEnd().EndsWith("FIN 36") && item.Row["Bind Type/# Panels/Rope/Operation"] == "Motorised")
                {
                    item.Row["Measured Width"] = (Convert.ToInt32(item.Row["Measured Width"]) - 5).ToString();
                }


                if (item.Row["Fabric Type"] != "")
                {
                    var fab = item.Row["Fabric Type"].TrimEnd();
                    if (FabricRollwidth.ContainsKey(fab.Substring(0, fab.LastIndexOf(' ')).TrimEnd()))
                    {
                        item.Row["Roll Width"] = FabricRollwidth[fab.Substring(0, fab.LastIndexOf(' ')).TrimEnd()].ToLower().Replace("mm", "");
                    }
                    else
                    {
                        item.Row["Roll Width"] = fab.Substring(fab.LastIndexOf(' ') + 1).Trim().ToLower().Replace("mm", "");
                    }

                }

                item.Row["Trim Type"] = "";
                if (!string.IsNullOrEmpty(item.Row["Description"].ToString().Trim()))
                    item.Row["Trim Type"] = item.Row["Description"].Substring(item.Row["Description"].ToString().Trim().Length - 6);


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
                item.Row["Barcode"] = item.Row["Line No"].ToString().TrimEnd();


                if (item.Row["Cntrl Side"].ToString().TrimEnd() != "")
                {
                    item.Row["Control Side"] = item.Row["Cntrl Side"].ToString().TrimEnd().ToCharArray()[0].ToString();// Added on 8-10-2016
                }
                else
                {
                    item.Row["Control Side"] = "N";

                }

                item.Row["FromHoldingStation"] = "NO";
            }

            foreach (var header in SelectedColumnsPath)
            {
                var Headertext = header.Replace(".", "");

                if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];

                if (!Data.ColumnNames.Contains(Headertext))
                    Data.ColumnNames.Add(Headertext);
            }

            Data.ColumnNames.Add("Roll Width");


        }

        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails([FromHeader] string CBNumber)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                Init();

                await ReadConfig();

                var check = CheckingPaths();
                if (!check) return new JsonResult(false);

                FabricCutterCBDetailsModel Data = ReadingData(CBNumber);

                FabricCutProcess(ref Data);

                if (!CBSearchOrLineNumberSearch)
                    Data.Rows = Data.Rows.Where(e => e.Row["Line No"] == CBNumber).ToList();

                return new JsonResult(Data);

            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }
        }

        public void PrintReport(string strPrinterName, string[] strParameterArray)
        {

            try
            {
                string mimtype = "";
                int extension = 1;
                var path = Path.Combine(_env.ContentRootPath, "Printer Driver", "FabricCutter.rdlc");

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");
                var parametersList = new List<ReportParameter>();

                parametersList.Add(new ReportParameter("someoftotal", strParameterArray[12].Split(" ")[1].ToString() + " of " + strParameterArray[13].ToString()));
                parametersList.Add(new ReportParameter("cutwidth", strParameterArray[10].Split(" ")[1].ToString()));
                parametersList.Add(new ReportParameter("controlside", strParameterArray[12].Split(" ")[0].ToString()));
                parametersList.Add(new ReportParameter("lineNumber", strParameterArray[11].ToString()));
                parametersList.Add(new ReportParameter("char", strParameterArray[10].Split(" ")[0].ToString()));
                parametersList.Add(new ReportParameter("lathe", strParameterArray[9].ToString()));
                parametersList.Add(new ReportParameter("controltype", strParameterArray[8].ToString()));
                parametersList.Add(new ReportParameter("color", strParameterArray[7].ToString()));
                parametersList.Add(new ReportParameter("fabric", strParameterArray[6].ToString()));
                parametersList.Add(new ReportParameter("type", strParameterArray[5].ToString()));
                parametersList.Add(new ReportParameter("department", strParameterArray[4].ToString()));
                parametersList.Add(new ReportParameter("customer", strParameterArray[3].ToString()));
                parametersList.Add(new ReportParameter("drop", strParameterArray[2].ToString() + " mm"));
                parametersList.Add(new ReportParameter("width", strParameterArray[1].ToString() + " mm"));
                parametersList.Add(new ReportParameter("cbNumber", strParameterArray[0].ToString()));



                LocalReport report = new LocalReport();
                report.ReportPath = path;
                report.SetParameters(parametersList);
                report.Refresh();
                byte[] result = report.Render("IMAGE");
                report.Dispose();

                var outputPath = Path.Combine(_env.ContentRootPath, "Printer Driver", "FabricCutterPrintFiles", Guid.NewGuid().ToString() + ".png");
                using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                {
                    stream.Write(result, 0, result.Length);
                }



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
                        m.Y = (int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Height - m.Height) / 2);
                        m.X = (int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Width - m.Width) / 2);
                        args.Graphics.DrawImage(i, m);
                    };
                    pd.Print();
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

        private void PrintPage(object o, PrintPageEventArgs e)
        {


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
                        PrintReport(strPrinterName, strParameterArray);
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
            var FBRSetting = await _repository.Tables.FindAsync(e => e.TableName == model.tableName);
            var FBRPath = FBRSetting.FirstOrDefault().OutputPath;

            if (FBRPath == "") return new JsonResult(false);
            DirectoryInfo F = new DirectoryInfo(FBRPath);
            if (!F.Exists) return new JsonResult(false);


            StringBuilder sb = new StringBuilder();
            StringBuilder labels = new StringBuilder();
            foreach (var item in Data.Rows)
            {
                if (item.Row["FromHoldingStation"] == "YES")
                    await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                        Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });
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
                bool res = await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), TableNumber, UserName, System.DateTime.Now.ToString(), item.Row["Alpha Number"], "FabricCut", item);


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
                labels.Append("@" + item.Row["Alpha Number"].ToString().TrimEnd() + " " + item.Row["Cut Width"].ToString().TrimEnd());
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
        public async Task<IActionResult> PrintLabelsOnly([FromBody] CreateFileAndLabelModel model)
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
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });
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

        public string CreateNewFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
            return destination;

        }

        [HttpGet("GetDataUsingAutoUpload")]
        public async Task<IActionResult> GetDataUsingAutoUpload([FromHeader] string TableName, [FromHeader] string UserName, [FromHeader] string Shift, [FromHeader] string Type)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Init();


                await ReadConfig();
                var check = CheckingPaths();

                if (!check) return new JsonResult(false);

                var AutoUploadDirSetting = await _repository.Settings.FindAsync(e => e.settingName == "AutoUploadDir");
                var AutoUploadDirPath = AutoUploadDirSetting.FirstOrDefault().settingPath;

                var ViewedUploadsSetting = await _repository.Settings.FindAsync(e => e.settingName == "ViewedUploadsDir");
                var ViewedUplaodsPath = ViewedUploadsSetting.FirstOrDefault().settingPath;

                var AutoUploadFolder = new DirectoryInfo(AutoUploadDirPath);
                if (AutoUploadFolder.Exists == false)
                    return new JsonResult(false);

                #region Reading Data

                /// first Get all Files that Starts with the TableName

                var files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains(TableName + "_" + Shift)).ToList();
                if (Type == "Urgent")
                    files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains("Urgent_" + TableName + "_" + Shift)).ToList();

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
                List<string> names = new List<string>();
                //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();

                generalBlindNumber = 1;
                foreach (var file in files)
                {
                    using (var package = new ExcelPackage(file))
                    {
                        var workbook = package.Workbook;

                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) return null;
                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        bool gotColumns = (SelectedColumnsPath.Count > 0) ? true : false;
                        var last = end.Column;
                        worksheet.Cells[1, last].Value = "done";
                        worksheet.Cells[1, last + 1].Value = "Processed By";
                        for (int i = start.Row + 1; i < end.Row; i++)
                        {
                            Dictionary<string, string> row = new Dictionary<string, string>();

                            row["Line No"] = "";
                            row["Customer"] = "";
                            row["Bind Type/# Panels/Rope/Operation"] = "";
                            row["Description"] = "";
                            row["Track Col/Roll Type/Batten Col"] = "";
                            row["Cntrl Side"] = "";
                            row["Control Type"] = "";
                            row["Pull Colour/Bottom Weight/Wand Len"] = "";
                            int RowQty = 0;
                            for (int j = start.Column; j < end.Column; j++)
                            {
                                var Headertext = worksheet.Cells[1, j].Text.Trim();
                                if (String.IsNullOrEmpty(Headertext)) continue;
                                if (Headertext.Contains("Qty"))
                                {
                                    if (String.IsNullOrEmpty(worksheet.Cells[i, j].Text.Trim()))
                                        continue;

                                    RowQty = int.Parse(worksheet.Cells[i, j].Text.Trim());
                                }

                                Headertext = Headertext.Replace(".", "");



                                var cell = worksheet.Cells[i, j].Text.Trim();

                                if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];
                                row[Headertext] = cell;


                            }
                            worksheet.Cells[i, last].Value = "No";
                            worksheet.Cells[i, last + 1].Value = UserName;
                            FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                            for (int cntr = generalBlindNumber; cntr < RowQty + generalBlindNumber; cntr++)
                            {
                                TblRow.BlindNumbers.Add(cntr);
                            }
                            generalBlindNumber += RowQty;
                            TblRow.Row = row;
                            TblRow.UniqueId = Guid.NewGuid().ToString();
                            TblRow.rows_AssociatedIds.Add(TblRow.UniqueId);

                            AutoUploadModel model = new AutoUploadModel
                            {
                                Id = TblRow.UniqueId,
                                FileName = file.Name,
                                CreationDate = file.CreationTime.ToString(),
                                row = TblRow,
                                Shift = Shift,
                                UserName = UserName,
                                TableName = TableName,
                                Type = Type
                            };

                            await _repository.AutoUploads.InsertOneAsync(model);
                        }


                        package.Dispose();
                    }

                    FileInfo checking = new FileInfo(Path.Combine(ViewedUplaodsPath, file.Name));
                    Random rd = new Random();


                    if (checking.Exists)
                        CreateNewFile(file.FullName, Path.Combine(ViewedUplaodsPath, rd.Next(1, 1000000).ToString() + "_" + file.Name));
                    else
                        CreateNewFile(file.FullName, Path.Combine(ViewedUplaodsPath, file.Name));
                    System.IO.File.Delete(file.FullName);

                }

                /// getting Data from db 

                var AutoUploadsModels = await _repository.AutoUploads.FindAsync(res => res.TableName == TableName && res.Shift == Shift && res.Type == Type).Result.ToListAsync();
                var FirstFileName = "";
                var FirstFileCreationTime = "";

                foreach (var item in AutoUploadsModels)
                {
                    if (FirstFileName == "")
                    {
                        FirstFileName = item.FileName;
                        FirstFileCreationTime = item.CreationDate;
                    }
                    if (FirstFileName != item.FileName || FirstFileCreationTime != item.CreationDate) break;

                    Data.Rows.Add(item.row);
                }

                ///
                Dictionary<string, string> FabricRollwidth = new Dictionary<string, string>();
                Dictionary<string, int> ControlTypevalues = new Dictionary<string, int>();
                Dictionary<string, List<string>> LatheType = new Dictionary<string, List<string>>();

                FBRPath = CreateNewFile(FBRPath, FBRPath.Substring(0, FBRPath.IndexOf(".")) + Guid.NewGuid().ToString() + FBRPath.Substring(FBRPath.IndexOf(".")));
                FileInfo fileTemp = new FileInfo(FBRPath);
                using (var package = new ExcelPackage(fileTemp))
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

                System.IO.File.Delete(FBRPath);


                DeductionPath = CreateNewFile(DeductionPath, DeductionPath.Substring(0, DeductionPath.IndexOf(".")) + Guid.NewGuid().ToString() + DeductionPath.Substring(DeductionPath.IndexOf(".")));

                fileTemp = new FileInfo(DeductionPath);
                using (var package = new ExcelPackage(fileTemp))
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

                System.IO.File.Delete(DeductionPath);

                LathePath = CreateNewFile(LathePath, LathePath.Substring(0, LathePath.IndexOf(".")) + Guid.NewGuid().ToString() + LathePath.Substring(LathePath.IndexOf(".")));

                fileTemp = new FileInfo(LathePath);
                using (var package = new ExcelPackage(fileTemp))
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

                System.IO.File.Delete(LathePath);

                #endregion


                #region Customization of columns

                var Bindcntr = -1;
                foreach (var item in Data.Rows)
                {
                    Bindcntr++;
                    item.Row["Total Blinds"] = (generalBlindNumber - 1).ToString();
                    if (item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd() != "")
                    {
                        if (ControlTypevalues.ContainsKey(item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()))
                        {
                            item.Row["Cut Width"] = (int.Parse(item.Row["Measured Width"]) + ControlTypevalues[item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()]).ToString();
                        }
                    }
                    else
                    {
                        var val = item.Row["Measured Width"] == "" ? "0" : item.Row["Measured Width"];
                        item.Row["Cut Width"] = (Convert.ToInt32(val) - 30).ToString();
                    }


                    if (item.Row["Description"].TrimEnd().EndsWith("FIN 36") && item.Row["Bind Type/# Panels/Rope/Operation"] == "Motorised")
                    {
                        item.Row["Measured Width"] = (Convert.ToInt32(item.Row["Measured Width"]) - 5).ToString();
                    }


                    if (item.Row["Fabric Type"] != "")
                    {
                        var fab = item.Row["Fabric Type"].TrimEnd();
                        if (FabricRollwidth.ContainsKey(fab.Substring(0, fab.LastIndexOf(' ')).TrimEnd()))
                        {
                            item.Row["Roll Width"] = FabricRollwidth[fab.Substring(0, fab.LastIndexOf(' ')).TrimEnd()].ToLower().Replace("mm", "");
                        }
                        else
                        {
                            item.Row["Roll Width"] = fab.Substring(fab.LastIndexOf(' ') + 1).Trim().ToLower().Replace("mm", "");
                        }

                    }


                    item.Row["Trim Type"] = "";
                    if (!string.IsNullOrEmpty(item.Row["Description"].ToString().Trim()))
                        item.Row["Trim Type"] = item.Row["Description"].Substring(item.Row["Description"].ToString().Trim().Length - 6);



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
                    item.Row["Barcode"] = item.Row["Line No"].ToString().TrimEnd();


                    if (item.Row["Cntrl Side"].ToString().TrimEnd() != "")
                    {
                        item.Row["Control Side"] = item.Row["Cntrl Side"].ToString().TrimEnd().ToCharArray()[0].ToString();// Added on 8-10-2016
                    }
                    else
                    {
                        item.Row["Control Side"] = "N";

                    }

                    item.Row["FromHoldingStation"] = "NO";
                }

                foreach (var header in SelectedColumnsPath)
                {
                    var Headertext = header.Replace(".", "");

                    if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];

                    if (!Data.ColumnNames.Contains(Headertext))
                        Data.ColumnNames.Add(Headertext);
                }

                #endregion

                #region getting Held Orders
                if (Type == "Normal")
                {
                    FabricCutterCBDetailsModel newdata = (FabricCutterCBDetailsModel)(await GetHeldObjects(TableName));
                    if (Data.Rows != null)
                    {
                        newdata.Rows.AddRange(Data.Rows);
                        newdata.ColumnNames = Data.ColumnNames;
                        Data = newdata;
                    }
                    else
                        Data = newdata;
                }
                #endregion
                
                Data.ColumnNames.Add("Roll Width");
                
                return new JsonResult(Data);

            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }
        }

        [HttpPost("UpdateRows")]
        public async Task<IActionResult> UpdateRows(List<string> ids)
        {

            try
            {
                foreach (var id in ids)
                {
                    await _repository.AutoUploads.DeleteOneAsync(entry => entry.Id == id);
                }

                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }


        }

        [HttpGet("GetHeldObjects")]
        public async Task<object> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();

                Init();

                await ReadConfig();

                var check = CheckingPaths();

                if (!check) return new JsonResult(false);




                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "FabricCut" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {

                    var HeldData = ReadingData(item.Row.Row["CB Number"]);

                    FabricCutProcess(ref HeldData);


                    Data.Rows.Add(HeldData.Rows.Where(e => e.Row["Line No"] == item.Row.Row["Line No"]).FirstOrDefault());
                    Data.Rows[Data.Rows.Count - 1].Row["FromHoldingStation"] = "YES";
                    Data.Rows[Data.Rows.Count - 1].UniqueId = item.Row.UniqueId;
                    Data.ColumnNames = HeldData.ColumnNames;
                }

                return Data;

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }

        [HttpPost("ClearOrdersFromFabricCutter")]
        public async Task<IActionResult> ClearOrdersFromFabricCutter([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                foreach (var item in model.data.Rows)
                {
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });

                    await _repository.AutoUploads.DeleteOneAsync(entry => entry.Id == item.UniqueId);
                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.Message);
            }

        }


    }
}
