﻿using AspNetCore.Reporting;
using AspNetCore.Reporting.ReportExecutionService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PrstringerProject;
using SautinSoft;
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
    public class FabricCutter : ControllerBase
    {

        public FabricCutter(IRepository repository, IWebHostEnvironment env)
        {

            _repository = repository;
            _env = env;
        }
        private IRepository _repository;

        private IWebHostEnvironment _env;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();
        string ctbsodumpPath, SheetNamePath, FBRPath, DeductionPath, LathePath;
        List<string> SelectedColumnsPath;
        Dictionary<string, string> FabricRollwidth;
        Dictionary<string, int> ControlTypevalues;
        Dictionary<string, List<string>> LatheType;
        Dictionary<string, int> ColumnsIndex;
        Dictionary<string, int> Color;
        bool CBSearchOrLineNumberSearch;
        string LineNumber;
        int generalBlindNumber;
        private void Init()
        {
            if (ColumnMapper.Count > 0) return;
            ColumnMapper.Add("Customer Name 1", "Customer");
            ColumnMapper.Add("W/Order NO", "CB Number");
            ColumnMapper.Add("Customer Name 2", "Supplier");
            ColumnMapper.Add("Qty", "Quantity");
            ColumnMapper.Add("Width", "Measured Width");
            ColumnMapper.Add("Drop", "Measured Drop");
            ColumnMapper.Add("Fabric", "Fabric Type");
            ColumnMapper.Add("Colour", "Fabric Colour");
            ColumnMapper.Add("Pull Type / Control Type /Draw Type", "Control Type");
            ColumnsIndex = new Dictionary<string, int>();
            Color = new Dictionary<string, int>();
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
                    for (int i = start.Column; i <= end.Column; i++)
                    {
                        var text = worksheet.Cells[1, i].Text.Trim();

                        if (text.Equals("W/Order NO")) CBINdex = i;
                        if (text.Equals("Line No."))
                        {
                            for (int j = start.Row + 1; j <= end.Row; j++)
                                if (worksheet.Cells[j, i].Text.Trim() == LineNumber) { CBNumber = worksheet.Cells[j, CBINdex].Text.Trim(); break; }
                        }
                    }
                }

                for (int i = start.Column; i <= end.Column; i++)
                {
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    ColumnsIndex[worksheet.Cells[1, i].Text.Trim()] = i;

                    if (Headertext.StartsWith("W/Order NO"))
                    {
                        for (int j = start.Row + 1; j <= end.Row; j++)
                            if (worksheet.Cells[j, i].Text.Trim() != CBNumber) indexToRemove[j] = 1;
                    }
                }

                for (int i = start.Row + 1; i <= end.Row; i++)
                {
                    if (indexToRemove.ContainsKey(i)) continue;
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    row["Line No"] = "";
                    row["Customer"] = "";
                    row["Supplier"] = "";
                    row["Address1"] = "";
                    row["Address2"] = "";
                    row["Address3"] = "";
                    row["PostCode"] = "";
                    row["Carrier"] = "";
                    row["Location"] = "";
                    row["Debtor Order Number"] = "";
                    row["Order Department"] = "";
                    row["Bind Type/# Panels/Rope/Operation"] = "";
                    row["Description"] = "";
                    row["Track Col/Roll Type/Batten Col"] = "";
                    row["Cntrl Side"] = "";
                    row["Control Type"] = "";
                    row["Pull Colour/Bottom Weight/Wand Len"] = "";


                    int RowQty = 0;
                    for (int j = start.Column; j <= end.Column; j++)
                    {
                        var Headertext = worksheet.Cells[1, j].Text.Trim();
                        if (String.IsNullOrEmpty(Headertext)) continue;
                        Headertext = Headertext.Replace(".", "");

                        if (Headertext.Contains("Qty"))
                        {
                            RowQty = int.Parse(worksheet.Cells[i, j].Text.Trim());
                        }

                        if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "")
                        { RowQty = 0; break; }


                        var cell = worksheet.Cells[i, j].Text.Trim();

                        if (Headertext == ("Colour"))
                        {
                            /// Adding Counting Color Logic 
                            var RowColor = (worksheet.Cells[i, j].Text.Trim());
                            if (RowColor != "")
                            {
                                if (!Color.ContainsKey(RowColor)) Color[RowColor] = 0;
                                Color[RowColor]++;
                            }
                        }

                        if (ColumnMapper.ContainsKey(Headertext))
                            Headertext = ColumnMapper[Headertext];
                        row[Headertext] = cell;

                    }
                    if (RowQty == 0)
                        continue;
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
                for (int i = start.Row + 1; i <= end.Row; i++)
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
                for (int i = start.Row + 1; i <= end.Row; i++)
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
                for (int i = start.Row + 1; i <= end.Row; i++)
                {
                    if (!LatheType.ContainsKey(worksheet.Cells[i, 1].Text.Trim()))
                        LatheType[worksheet.Cells[i, 1].Text.Trim()] = new List<string>();

                    LatheType[worksheet.Cells[i, 1].Text.Trim()].Add(worksheet.Cells[i, 2].Text.Trim());

                }
            }

            System.IO.File.Delete(TempLathePath);


            return Data;


        }

        private void FabricCutProcess(ref FabricCutterCBDetailsModel Data)
        {
            var Bindcntr = -1;
            foreach (var item in Data.Rows)
            {
                Bindcntr++;
                item.Row["RowColorCount"] = "0";
                var RowColor = item.Row["Fabric Colour"].Trim();
                if (RowColor != "")
                    item.Row["RowColorCount"] = Color[RowColor].ToString();

                item.Row["Total Blinds"] = (generalBlindNumber - 1).ToString();
                item.Row["Total"] = (generalBlindNumber - 1).ToString();
                item.Row["Cut Width"] = "0";
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
        public async Task<ResultModel> getCBNumberDetails([FromHeader] string CBNumber)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                Init();

                await ReadConfig();

                var check = CheckingPaths();
                if (!check) return (ResultModel)new ResultModel
                {
                    Message = "Missing configuration files !",
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                };

                FabricCutterCBDetailsModel Data = ReadingData(CBNumber);

                if (Data == null)
                {
                    return (ResultModel)new ResultModel
                    {
                        Message = "Something wrong with the ctbsodumpfile",
                        Data = null,
                        Status = System.Net.HttpStatusCode.BadRequest,
                        StackTrace = null
                    };
                }

                if (Data.Rows.Count == 0)
                {
                    return (ResultModel)new ResultModel
                    {
                        Message = "CC/line number is not found",
                        Data = null,
                        Status = System.Net.HttpStatusCode.BadRequest,
                        StackTrace = null
                    };
                };

                FabricCutProcess(ref Data);

                if (!CBSearchOrLineNumberSearch)
                    Data.Rows = Data.Rows.Where(e => e.Row["Line No"] == CBNumber).ToList();

                return Repository.ReturnSuccessfulRequest(Data);

            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }

        public string PrintReport(string strPrinterName, string[] strParameterArray)
        {


            string mimtype = "";
            int extension = 1;
            var path = Path.Combine("E:\\Webapp_input files", "Printer Driver", "FabricCutter.rdlc");
            //path = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject", "FabricCutter.rdlc");
            AspNetCore.Reporting.LocalReport report = new AspNetCore.Reporting.LocalReport(path);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("us-ascii");

            FabricCut obj = new FabricCut();
            for (int i = 0; i < strParameterArray.Length; i++)
            {
                while (strParameterArray[i].IndexOf("  ") != -1)
                    strParameterArray[i] = strParameterArray[i].Replace("  ", " ");
                if (String.IsNullOrEmpty(strParameterArray[i]))
                    strParameterArray[i] = " ";
            }

            obj.someoftotal = strParameterArray[12].Split(" ")[1].ToString() + " of " + strParameterArray[13].ToString();
            obj.cutwidth = strParameterArray[10].Split(" ")[1].ToString();
            obj.cntrside = strParameterArray[12].Split(" ")[0].ToString();
            obj.lineNumber = strParameterArray[11].ToString();
            obj.c = strParameterArray[10].Split(" ")[0].ToString();
            obj.lathe = strParameterArray[9].ToString();
            obj.controltype = strParameterArray[8].ToString();
            obj.color = strParameterArray[7].ToString();
            obj.fabric = strParameterArray[6].ToString();
            obj.type = strParameterArray[5].ToString();
            obj.department = strParameterArray[4].ToString();
            obj.customer = strParameterArray[3].ToString();
            obj.drop = strParameterArray[2].ToString() + " mm";
            obj.width = strParameterArray[1].ToString() + " mm";
            obj.cbNumber = strParameterArray[0].ToString();

            List<FabricCut> ls = new List<FabricCut> {
                    obj
                    };
            report.AddDataSource("FabricCut", ls);

            byte[] result = report.Execute(RenderType.Pdf, extension, null, mimtype).MainStream;

            var outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "FabricCutterPrintFiles", Guid.NewGuid().ToString() + ".pdf");
            //outputPath = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject\\Delete", Guid.NewGuid().ToString() + ".pdf");
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
            doc.PrintSettings.SetPaperMargins(0, 0, 0, 0);
            doc.PrintSettings.SelectPageRange(1, 1);
            doc.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize, false);
            doc.PrintSettings.Landscape = false;
            //doc.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.ActualSize);
            doc.Print();

            return "";

        }

        public string PrintLabels(string strParameter)
        {

            string[] strpara = null;
            strpara = strParameter.Replace("|@", "|").Split('|');
            var Error = "";

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
                        Error = PrintReport(strPrinterName, strParameterArray);
                    }
                }

            }
            return Error;
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
        public async Task<ResultModel> CreateFilesAndLabels([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                var Data = model.data;
                var TableNumber = model.tableName;
                var UserName = model.userName;
                var PrinterName = model.printer;
                var FBRSetting = await _repository.Tables.FindAsync(e => e.TableName == model.tableName);
                var FBRPath = FBRSetting.FirstOrDefault().OutputPath;

                if (FBRPath == "") return (ResultModel)new ResultModel
                {
                    Message = "Missing configuration for FabricCut go to settings page",
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                };
                DirectoryInfo F = new DirectoryInfo(FBRPath);
                if (!F.Exists) return (ResultModel)new ResultModel
                {
                    Message = "FabricCut output folder is not found",
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                };


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
                    sb.Append("CONTROL TYPE\t" + (item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd() == "" ? "Pin" : item.Row["Bind Type/# Panels/Rope/Operation"].ToString().TrimEnd()) + Environment.NewLine);
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
                    // labels.Append("@" + item.Row["Department"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["RowColorCount"].ToString().TrimEnd());


                    if (item.Row["Track Colour"].ToString().TrimEnd() == "OVEROLL")
                    {
                        labels.Append("@O/R");
                    }
                    else
                    {
                        labels.Append("@" + item.Row["Track Colour"].ToString().TrimEnd());
                    }

                    item.Row["Alpha Number"] = CalculateAlphabeticFromNumber(int.Parse(item.Row["Blind Number"]));
                    item.Row["SomeOfTotal"] = item.Row["Blind Number"].ToString().Trim() + " of " + item.Row["Total Blinds"].ToString().Trim();
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

                return Repository.ReturnSuccessfulRequest(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Repository.ReturnBadRequest(e);
            }

        }

        [HttpPost("PrintLabelsOnly")]
        public async Task<ResultModel> PrintLabelsOnly([FromBody] CreateFileAndLabelModel model)
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
                    //labels.Append("@" + item.Row["Department"].ToString().TrimEnd());
                    labels.Append("@" + item.Row["RowColorCount"].ToString().TrimEnd());

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

                var error = PrintLabels(PrinterName + "|" + labeldata);
                return (error.Trim() == "" ? Repository.ReturnSuccessfulRequest(true) : (ResultModel)new ResultModel
                {
                    Message = error,
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                });
            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }

        public string CreateNewFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
            return destination;

        }

        [HttpGet("GetDataUsingAutoUpload")]
        public async Task<ResultModel> GetDataUsingAutoUpload([FromHeader] string TableName, [FromHeader] string UserName, [FromHeader] string Shift, [FromHeader] string Type)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Init();

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();

                await ReadConfig();
                var check = CheckingPaths();

                if (!check) return (ResultModel)new ResultModel
                {
                    Message = "Missing configuration files !",
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                };

                var AutoUploadDirSetting = await _repository.Settings.FindAsync(e => e.settingName == "AutoUploadDir" && e.applicationSetting == "_FabricCutter");
                var AutoUploadDirPath = AutoUploadDirSetting.FirstOrDefault().settingPath;

                var ViewedUploadsSetting = await _repository.Settings.FindAsync(e => e.settingName == "ViewedUploadsDir" && e.applicationSetting == "_FabricCutter");
                var ViewedUplaodsPath = ViewedUploadsSetting.FirstOrDefault().settingPath;

                var AutoUploadFolder = new DirectoryInfo(AutoUploadDirPath);
                if (AutoUploadFolder.Exists == false)
                    return (ResultModel)new ResultModel
                    {
                        Message = "AutoUploadFolder doesn't exist",
                        Data = null,
                        Status = System.Net.HttpStatusCode.BadRequest,
                        StackTrace = null
                    };

                var AutoUploadErrorsPath = ViewedUplaodsPath.Substring(0, ViewedUplaodsPath.LastIndexOf(Path.DirectorySeparatorChar.ToString())) + Path.DirectorySeparatorChar.ToString() + "AutoUploadErrorFolder";

                var ErrorDirectory = new DirectoryInfo(AutoUploadErrorsPath);
                if (!ErrorDirectory.Exists)
                    ErrorDirectory.Create();


                #region Reading Data

                /// first Get all Files that Starts with the TableName

                var files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains(TableName + "_" + Shift) && !file.Name.Contains("Urgent")).ToList();
                if (Type == "Urgent")
                    files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains("Urgent_" + TableName + "_" + Shift)).ToList();

                List<string> names = new List<string>();


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
                    for (int i = start.Row + 1; i <= end.Row; i++)
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
                    for (int i = start.Row + 1; i <= end.Row; i++)
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
                    for (int i = start.Row + 1; i <= end.Row; i++)
                    {
                        if (!LatheType.ContainsKey(worksheet.Cells[i, 1].Text.Trim()))
                            LatheType[worksheet.Cells[i, 1].Text.Trim()] = new List<string>();

                        LatheType[worksheet.Cells[i, 1].Text.Trim()].Add(worksheet.Cells[i, 2].Text.Trim());

                    }
                }

                System.IO.File.Delete(LathePath);

                #endregion

                generalBlindNumber = 1;
                foreach (var file in files)
                {
                    try
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

                            for (int i = start.Row + 1; i <= end.Row; i++)
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
                                row["BlueSleeve"] = "";
                                int RowQty = 0;
                                for (int j = start.Column; j <= end.Column; j++)
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


                                    if (Headertext == ("Colour"))
                                    {
                                        /// Adding Counting Color Logic 
                                        var RowColor = (worksheet.Cells[i, j].Text.Trim());
                                        if (RowColor != "")
                                        {
                                            if (!Color.ContainsKey(RowColor)) Color[RowColor] = 0;
                                            Color[RowColor]++;
                                        }
                                    }

                                    if (ColumnMapper.ContainsKey(Headertext))
                                        Headertext = ColumnMapper[Headertext];
                                    row[Headertext] = cell;

                                }

                                FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                                for (int cntr = generalBlindNumber; cntr < RowQty + generalBlindNumber; cntr++)
                                {
                                    TblRow.BlindNumbers.Add(cntr);
                                }
                                generalBlindNumber += RowQty;
                                TblRow.Row = row;
                                TblRow.UniqueId = Guid.NewGuid().ToString();
                                TblRow.rows_AssociatedIds.Add(TblRow.UniqueId);
                                TblRow.FileName = file.Name;
                                TblRow.CreationDate = file.CreationTime.ToString();

                                Data.Rows.Add(TblRow);
                            }


                            package.Dispose();
                        }


                        #region Customization of Data

                        var Bindcntr = -1;
                        foreach (var item in Data.Rows)
                        {
                            Bindcntr++;

                            /// Adding Counting Color Logic 
                            item.Row["RowColorCount"] = "0";
                            var RowColor = item.Row["Fabric Colour"].Trim();
                            if (RowColor != "")
                                item.Row["RowColorCount"] = Color[RowColor].ToString();

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
                                if (FabricRollwidth.ContainsKey(fab.Substring(0, fab.LastIndexOf(' ') == -1 ? fab.Length : fab.LastIndexOf(' ')).TrimEnd()))
                                {
                                    item.Row["Roll Width"] = FabricRollwidth[fab.Substring(0, fab.LastIndexOf(' ') == -1 ? fab.Length : fab.LastIndexOf(' ')).TrimEnd()].ToLower().Replace("mm", "");
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

                        #endregion


                        #region Removing The file

                        FileInfo checking = new FileInfo(Path.Combine(ViewedUplaodsPath, file.Name));
                        Random rd = new Random();


                        if (checking.Exists)
                            CreateNewFile(file.FullName, Path.Combine(ViewedUplaodsPath, rd.Next(1, 1000000).ToString() + "_" + file.Name));
                        else
                            CreateNewFile(file.FullName, Path.Combine(ViewedUplaodsPath, file.Name));
                        System.IO.File.Delete(file.FullName);

                        #endregion

                        #region Adding the processed Data to the DB

                        foreach (var item in Data.Rows)
                        {
                            AutoUploadModel model = new AutoUploadModel
                            {
                                Id = item.UniqueId,
                                FileName = item.FileName,
                                CreationDate = item.CreationDate,
                                row = item,
                                Shift = Shift,
                                UserName = UserName,
                                TableName = TableName,
                                Type = Type,
                                Station = "FabricCut"
                            };

                            await _repository.AutoUploads.InsertOneAsync(model);
                        }

                        #endregion

                        Data.Rows.Clear();

                    }
                    catch (Exception)
                    {

                        FileInfo checking = new FileInfo(Path.Combine(ViewedUplaodsPath, file.Name));
                        Random rd = new Random();


                        if (checking.Exists)
                            CreateNewFile(file.FullName, Path.Combine(AutoUploadErrorsPath, rd.Next(1, 1000000).ToString() + "_" + file.Name));
                        else
                            CreateNewFile(file.FullName, Path.Combine(AutoUploadErrorsPath, file.Name));
                        System.IO.File.Delete(file.FullName);
                    }
                }





                foreach (var header in SelectedColumnsPath)
                {
                    var Headertext = header.Replace(".", "");

                    if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];

                    if (!Data.ColumnNames.Contains(Headertext))
                        Data.ColumnNames.Add(Headertext);
                }


                #region reading data from autouploads db

                var AutoUploadsModels = await _repository.AutoUploads.FindAsync(res => res.TableName == TableName && res.Shift == Shift && res.Type == Type && res.Station == "FabricCut").Result.ToListAsync();
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


                #endregion


                #region getting Held Orders

                if (Type == "Normal")
                {
                    FabricCutterCBDetailsModel newdata = (FabricCutterCBDetailsModel)(await GetHeldObjects(TableName)).Data;
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

                return Repository.ReturnSuccessfulRequest(Data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);

                return Repository.ReturnBadRequest(e);
            }
        }

        [HttpPost("UpdateRows")]
        public async Task<ResultModel> UpdateRows(List<string> ids)
        {

            try
            {
                foreach (var id in ids)
                {
                    await _repository.AutoUploads.DeleteOneAsync(entry => entry.Id == id);
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

                Init();

                await ReadConfig();

                var check = CheckingPaths();

                if (!check) return (ResultModel)new ResultModel
                {
                    Message = "Missing configuration files !",
                    Data = null,
                    Status = System.Net.HttpStatusCode.BadRequest,
                    StackTrace = null
                };




                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "FabricCut" && rej.TableName == tableName).Result.ToListAsync();

                Parallel.ForEach(HeldObjects, item =>
                 {
                     var HeldData = ReadingData(item.Row.Row["CB Number"]);

                     FabricCutProcess(ref HeldData);
                     if (HeldData.Rows.Count != 0)

                     {
                         Data.Rows.Add(HeldData.Rows.Where(e => e.Row["Line No"] == item.Row.Row["Line No"]).FirstOrDefault());
                         Data.Rows[Data.Rows.Count - 1].Row["FromHoldingStation"] = "YES";
                         Data.Rows[Data.Rows.Count - 1].UniqueId = item.Row.UniqueId;
                         Data.ColumnNames = HeldData.ColumnNames;
                     }
                 });

                return Repository.ReturnSuccessfulRequest(Data);

            }
            catch (Exception e)
            {
                return Repository.ReturnBadRequest(e);
            }
        }

        [HttpPost("ClearOrdersFromFabricCutter")]
        public async Task<ResultModel> ClearOrdersFromFabricCutter([FromBody] CreateFileAndLabelModel model)
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
                return Repository.ReturnSuccessfulRequest(true);
            }
            catch (Exception e)
            {

                return Repository.ReturnBadRequest(e);
            }

        }


    }
}
