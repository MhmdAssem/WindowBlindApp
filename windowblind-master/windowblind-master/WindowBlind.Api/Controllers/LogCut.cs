using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
    public class LogCut : ControllerBase
    {

        public LogCut(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;

        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails([FromHeader] string CBNumberOrLineNumber, [FromHeader] string CBorLine)
        {
            try
            {
                bool CBSearch = CBorLine == "CB" ? true : false;
                var CB = CBNumberOrLineNumber;
                var LineNumber = CBNumberOrLineNumber;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Dictionary<string, int> ColumnsIndex = new Dictionary<string, int>();


                #region Reading the config
                /// get the dumb file Path
                var ctbsodumpSetting = await _repository.Settings.FindAsync(e => e.settingName == "ctbsodump");
                var ctbsodumpPath = ctbsodumpSetting.FirstOrDefault().settingPath;

                /// get the sheet name
                var SheetNameSetting = await _repository.Settings.FindAsync(e => e.settingName == "SheetName");
                var SheetNamePath = SheetNameSetting.FirstOrDefault().settingPath;

                /// get Fabric Roll WIdth File

                var FBRSetting = await _repository.Settings.FindAsync(e => e.settingName == "Fabric Rollwidth");
                var FBRPath = FBRSetting.FirstOrDefault().settingPath;

                /// get deduct width file
                var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "DeductionTable");
                var DeductionPath = DeductionSetting.FirstOrDefault().settingPath;

                ///get Lathe file path
                var LatheSetting = await _repository.Settings.FindAsync(e => e.settingName == "PVCLathe Fabric");
                var LathePath = LatheSetting.FirstOrDefault().settingPath;

                /// get the Fabric data 
                var FabricSetting = await _repository.Settings.FindAsync(e => e.settingName == "FabricTable");
                var FabricPath = FabricSetting.FirstOrDefault().settingPath;

                /// get the Fabric data 
                var DropSetting = await _repository.Settings.FindAsync(e => e.settingName == "DropTable");
                var DropPath = DropSetting.FirstOrDefault().settingPath;

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

                file = new FileInfo(FabricPath);
                if (!file.Exists) return new JsonResult(false);

                file = new FileInfo(DropPath);
                if (!file.Exists) return new JsonResult(false);

                #endregion

                #region Reading Data
                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
                file = new FileInfo(ctbsodumpPath);
                if (!file.Exists) return null;
                List<string> names = new List<string>();
                //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
                int generalBlindNumber = 1;


                Dictionary<string, string> FabricRollwidth = new Dictionary<string, string>();
                Dictionary<string, int> ControlTypevalues = new Dictionary<string, int>();
                Dictionary<string, List<string>> LatheType = new Dictionary<string, List<string>>();
                List<DropTableModel> Droptable = new List<DropTableModel>();
                List<string> FabricDatable = new List<string>();

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

                file = new FileInfo(FabricPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        var text = worksheet.Cells[i, 1].Text.Trim();
                        if (!FabricDatable.Contains(text))
                            FabricDatable.Add(text);
                    }
                }

                file = new FileInfo(DropPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i < end.Row; i++)
                    {
                        DropTableModel model = new DropTableModel
                        {
                            From = int.Parse(worksheet.Cells[i, 1].Text.Trim()),
                            To = int.Parse(worksheet.Cells[i, 2].Text.Trim()),
                            DropGroup = worksheet.Cells[i, 3].Text.Trim(),
                            DropColour = worksheet.Cells[i, 4].Text.Trim()
                        };
                        Droptable.Add(model);
                    }
                }

                var TotalQty = 0;
                file = new FileInfo(ctbsodumpPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.Where(e => e.Name == SheetNamePath).FirstOrDefault();
                    if (worksheet == null) return null;
                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;

                    Dictionary<int, int> indexToRemove = new Dictionary<int, int>();

                    if (!CBSearch)
                    {
                        LineNumber = CBNumberOrLineNumber;
                        var CBINdex = 0;
                        for (int i = start.Column; i < end.Column; i++)
                        {
                            var text = worksheet.Cells[1, i].Text.Trim();
                            if (text.Equals("W/Order NO")) CBINdex = i;
                            if (text.Equals("Line No."))
                            {
                                for (int j = start.Row + 1; j < end.Row; j++)
                                    if (worksheet.Cells[j, i].Text.Trim() == LineNumber) CB = worksheet.Cells[j, CBINdex].Text.Trim();
                                break;
                            }
                        }
                    }

                    for (int i = start.Column; i < end.Column; i++)
                    {
                        var text = worksheet.Cells[2, i].Text.Trim();
                        ColumnsIndex[worksheet.Cells[1, i].Text.Trim()] = i;
                        if (text.StartsWith("CB"))
                        {
                            for (int j = start.Row + 1; j < end.Row; j++)
                                if (worksheet.Cells[j, i].Text.Trim() != CB) indexToRemove[j] = 1;

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

                            Headertext = Headertext.Replace(".", "");
                            /// special check 
                            if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "") continue;
                            var cell = worksheet.Cells[i, j].Text.Trim();

                            if (Headertext.Contains("Qty") && cell != "")
                            {
                                TotalQty += int.Parse(cell);
                            }

                            row[Headertext] = cell;

                        }

                        FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                        TblRow.Row = row;
                        TblRow.UniqueId = Guid.NewGuid().ToString();
                        Data.Rows.Add(TblRow);
                    }
                    package.Dispose();
                }


                #endregion

                FabricCutterCBDetailsModel FinalizedData = new FabricCutterCBDetailsModel();

                #region Customization of columns

                var f = 0;
                var a = 0;
                var Fbindex = 0;
                var blindNumber = 1;
                var rowCntr = -1;
                foreach (var item in Data.Rows)
                {
                    rowCntr++;
                    Fbindex = 0;
                    item.Row["CB Number"] = item.Row["W/Order NO"].Trim();
                    item.Row["Fabric"] = item.Row["Fabric"];

                    if (item.Row["Fabric"] != "")
                    {
                        for (int n = 0; n < FabricDatable.Count; n++)
                        {
                            if (item.Row["Fabric"].ToUpper().Trim().Contains(FabricDatable[n].ToUpper()))
                            {
                                Fbindex = 1;

                                break;
                            }

                        }
                        f += 1;
                        a += 1;
                    }
                    else
                    {
                        Fbindex = 0;

                    }

                    // WILL ADD CHAR ABCD FOR ITEM NUMBER

                    if (Fbindex > 0)
                    {
                        if (CBSearch)
                        {
                            item.Row["item"] = CalculateAlphabeticFromNumber(f);
                        }
                        else
                        {
                            a = 1;
                            f = 1;
                            for (int j = 0; j < Data.Rows.Count; j++)
                            {
                                if (Data.Rows[j].Row["Line No"] == item.Row["Line No"])
                                {
                                    item.Row["item"] = CalculateAlphabeticFromNumber(f); break;
                                }
                                f = f + 1;
                                a = a + 1;
                            }

                        }
                        item.Row["Total"] = TotalQty.ToString();

                    }
                    else
                        continue;

                    if (item.Row["Drop"] != String.Empty && int.TryParse(item.Row["Drop"].Trim(), out int res))
                    {
                        var val = Droptable.Where(d => int.Parse(item.Row["Drop"].Trim()) >= d.From && int.Parse(item.Row["Drop"].Trim()) <= d.To).FirstOrDefault();
                        if (val != null)
                        {
                            item.Row["DropGroup"] = val.DropGroup;
                            item.Row["DropColour"] = val.DropColour.ToLower();
                        }

                    }

                    if (item.Row["Width"] != "")
                        item.Row["Width"] = item.Row["Width"] + "mm";
                    else
                        item.Row["Width"] = "0";

                    item.Row["CutWidth"] = GetCutwidth2(item.Row["Width"], item.Row["Bind Type/# Panels/Rope/Operation"], ControlTypevalues);
                    item.Row["CutWidth_hidden"] = item.Row["CutWidth"];
                    if (item.Row["CutWidth"] != String.Empty)
                        item.Row["CutWidth"] = item.Row["CutWidth"] + "mm";
                    else
                        item.Row["CutWidth"] = "0";

                    item.Row["B_RColour"] = item.Row["Pull Colour/Bottom Weight/Wand Len"].ToString().Trim();
                    item.Row["ImgChecked"] = "0";

                    item.Row["Qty"] = item.Row["Qty"].Trim();


                    item.Row["Date-Time"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    item.Row["Tube"] = item.Row["Tube Size"].Trim();

                    if (item.Row["Bind Type/# Panels/Rope/Operation"].ToString().Trim() == "SPRING" || item.Row["Bind Type/# Panels/Rope/Operation"].ToString().Trim() == "SPRINGHD")
                        item.Row["Spring"] = "YES";
                    else
                        item.Row["Spring"] = "NO";


                    if (item.Row["Description"].ToString().Trim().Length > 6)
                        item.Row["Finish"] = item.Row["Description"].ToString().Trim().Substring(item.Row["Description"].ToString().Trim().Length - 6);
                    else
                    {
                        if (item.Row.ContainsKey("Finish"))
                        {
                            item.Row["Finish"] = item.Row["Finish"]; // there is no FInish in the file !
                        }
                    }
                    item.Row["Colour"] = item.Row["Colour"].Trim();


                    //Data for Label
                    item.Row["SRLineNumber"] = a.ToString();
                    if (item.Row["Drop"] != string.Empty)
                    {
                        if (item.Row["Width"].ToString() != string.Empty)
                            item.Row["Drop"] = item.Row["Drop"] + "mm";
                    }
                    else
                        item.Row["Drop"] = "0";

                    item.Row["Customer"] = item.Row["Customer Name 1"].Trim();
                    item.Row["Type"] = item.Row["Track Col/Roll Type/Batten Col"].Trim();
                    item.Row["Control Type"] = item.Row["Pull Colour/Bottom Weight/Wand Len"].Trim();
                    item.Row["Lathe"] = item.Row["Finish"].Trim();
                    item.Row["Alpha"] = item.Row["item"];
                    item.Row["Department"] = item.Row["Department"].Trim();
                    if (item.Row["Cntrl Side"].ToString().Trim() != String.Empty)
                        item.Row["ControlSide"] = item.Row["Cntrl Side"].ToString().Trim().Substring(0, 1);
                    item.Row["Barcode"] = item.Row["Line No"];

                    var CurrentWidth = item.Row["Width"];
                    if (CurrentWidth.IndexOf("mm") != -1)
                        CurrentWidth = CurrentWidth.Substring(0, CurrentWidth.IndexOf("mm"));
                    var CurrentDrop = item.Row["Drop"];
                    if (CurrentDrop.IndexOf("mm") != -1)
                        CurrentDrop = CurrentDrop.Substring(0, CurrentDrop.IndexOf("mm"));



                    if (int.TryParse(CurrentWidth, out int tempWidth) && int.TryParse(CurrentDrop, out int tempDrop) && tempWidth <= 3000 && tempDrop <= 2700)
                    {
                        item.Row["Fixing_Type_Bracket_Type"] = item.Row["Fixing Type / Bracket Type"];

                        if (Fbindex > 0)
                        {
                            if (CBSearch)
                            {

                                var Quantity = int.Parse(item.Row["Qty"]);
                                item.Row["Qty"] = "1";
                                for (int j = 0; j < Quantity; j++)
                                {
                                    var FinalRow = new FabricCutterCBDetailsModelTableRow();
                                    FinalRow = item;
                                    FinalRow.Row["Blind Number"] = (blindNumber).ToString();
                                    FinalRow.Row["SRLineNumber"] = blindNumber.ToString();

                                    blindNumber++;
                                    a += 1;
                                    FinalizedData.Rows.Add(FinalRow);

                                }
                            }
                            else
                            {
                                if (item.Row["Line No"] == LineNumber)
                                {
                                    var Quantity = int.Parse(item.Row["Qty"]);
                                    item.Row["Qty"] = "1";
                                    for (int j = 0; j < Quantity; j++)
                                    {
                                        var FinalRow = new FabricCutterCBDetailsModelTableRow();
                                        FinalRow = item;
                                        FinalRow.Row["Blind Number"] = (blindNumber).ToString();
                                        FinalRow.Row["SRLineNumber"] = blindNumber.ToString();
                                        blindNumber++;
                                        a += 1;
                                        
                                        FinalizedData.Rows.Add(FinalRow);
                                    }
                                }
                            }
                        }
                    }
                }

                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "CB Number");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "item");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Fabric");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Colour");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Drop");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Tube");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Date-Time");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Width");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "CutWidth");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "B_RColour");
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Line No");

                #endregion
                return new JsonResult(FinalizedData);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }

        [HttpPost("LogCutSend")]
        public async Task<IActionResult> LogCutSend(CreateFileAndLabelModel model)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var LogCutOutputSettings = await _repository.Settings.FindAsync(e => e.settingName == "LogCut Output");
                var LogCutOutputPath = LogCutOutputSettings.FirstOrDefault().settingPath;
                if (LogCutOutputPath == "") return new JsonResult(false);
                DirectoryInfo f = new DirectoryInfo(LogCutOutputPath);

                if (!f.Exists) return new JsonResult(false);

                if (model.printer == null || model.printer == "") return new JsonResult(false);

                var data = model.data;
                var printerName = model.printer;
                var tableName = model.tableName;
                var strconcat = "";
                List<string> labels = new List<string>();
                var strRS232Width = "";
                foreach (var item in data.Rows)
                {
                    strconcat = item.Row["CB Number"] + "@" + item.Row["Width"];
                    strconcat += "@" + item.Row["Drop"] + "@" + item.Row["Customer"] + "@" + item.Row["Department"];
                    strconcat += "@" + item.Row["Type"] + "@" + item.Row["Fabric"] + "@" + item.Row["Colour"];
                    strconcat += "@" + item.Row["Control Type"] + "@" + item.Row["Lathe"];
                    strconcat += "@" + item.Row["Alpha"] + "@" + item.Row["CB Number"] + "@" + item.Row["SRLineNumber"];
                    strconcat += "@" + item.Row["Total"];
                    strconcat += "@" + item.Row["CutWidth"].Replace("mm", "");
                    strconcat += "@" + item.Row["Line No"];
                    strconcat += "@" + item.Row["ControlSide"];
                    labels.Add(strconcat);
                    strRS232Width += item.Row["CutWidth"].ToString().Replace("mm", "");

                    bool res = await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), model.tableName, model.userName, System.DateTime.Now.ToShortDateString(), item.Row["Alpha"], "LogCut", item);

                }
                // doing the port thing 
                ComportModel comport = new ComportModel
                {
                    username = model.userName,
                    tablename = model.tableName,
                    date = DateTime.Now.ToString(),
                    id = Guid.NewGuid().ToString(),
                    status = "New",
                    value = strRS232Width,
                    applicationtype = "LogCut"
                };

                await _repository.comport.InsertOneAsync(comport);

                // add to file 

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                /// get old style


                LogCutOutputPath = Path.Combine(LogCutOutputPath, "LogCutOutput" + DateTime.Now.ToString("dd-mm-yyyy  hh-mm-ss") + ".xlsx");
                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Logcut");


                wsSheet1.Cells["A1:I1"].Merge = true;

                wsSheet1.Cells[1, 1].Value = "Logcut";
                wsSheet1.Cells[1, 1].Style.Font.Size = 20;
                wsSheet1.Cells[1, 1].Style.Font.Italic = true;
                wsSheet1.Cells[1, 1].Style.Font.UnderLine = true;
                wsSheet1.Cells[1, 1].Style.Font.UnderLineType = ExcelUnderLineType.Double;
                wsSheet1.Cells[1, 1].Style.Font.Name = "Calibri";
                wsSheet1.Cells[1, 1].Style.Font.Bold = true;
                wsSheet1.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                wsSheet1.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                wsSheet1.Cells[1, 1].Style.Font.Color.SetColor(Color.Red);

                int cntr = 1;
                foreach (var col in data.ColumnNames)
                {
                    wsSheet1.Cells[2, cntr].Value = col;
                    wsSheet1.Cells[2, cntr].Style.Font.Bold = true;
                    wsSheet1.Cells[2, cntr].Style.Font.Size = 15;
                    wsSheet1.Cells[2, cntr].Style.Font.Color.SetColor(Color.Plum);

                    wsSheet1.Cells[2, cntr].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells[2, cntr].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells[2, cntr].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells[2, cntr].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    wsSheet1.Row(2).Height = 20;
                    cntr++;
                }
                var cntrRow = 3;
                var cntrCol = 1;
                cntr = 0;
                foreach (var item in data.Rows)
                {
                    cntrCol = 1;
                    foreach (var col in data.ColumnNames)
                    {
                        wsSheet1.Column(cntrCol).Width = 15;
                        wsSheet1.Cells[cntrRow, cntrCol].Value = item.Row[col];
                        wsSheet1.Cells[cntrRow, cntrCol].Style.Font.Size = 11;
                        wsSheet1.Cells[cntrRow, cntrCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[cntrRow, cntrCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[cntrRow, cntrCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[cntrRow, cntrCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[cntrRow, cntrCol].Style.WrapText = true;

                        cntrCol++;
                    }
                    wsSheet1.Row(cntrRow).Height = 30;
                    cntrRow++;

                }

                ExcelPkg.SaveAs(new FileInfo(LogCutOutputPath));

                // the printing

                for (int k = 0; k < labels.Count; k++)
                {
                    var strParameterArray = labels[k].ToString().Split("@");
                    var strpara = strParameterArray;


                    if (strpara.Length == 0) continue;

                    var ret1 = PrintReport(printerName, strParameterArray.ToList(), "LogCut1.rdlc", "Width");
                    var ret2 = PrintReport(printerName, strParameterArray.ToList(), "LogCut2.rdlc", "");

                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }


        }
 
        public string GetCutwidth2(string width, string value, Dictionary<string, int> ControlTypevalues)
        {
            if (width == String.Empty || int.TryParse(width, out int res) == false)
                return width;
            else
            {
                if (int.Parse(width) == 0)
                    return width;

            }
            if (value.Trim() == String.Empty)
                value = "BLANK";


            var DeductedValue = 0;
            if (ControlTypevalues.ContainsKey(value.Trim()))
                DeductedValue = ControlTypevalues[value.Trim()];

            string cutwidth;
            if (DeductedValue != 0)
                cutwidth = (Double.Parse(width) - DeductedValue).ToString();
            else
                cutwidth = width;

            return cutwidth;
        }
        public static string CalculateAlphabeticFromNumber(int columnNumber)
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
        public static List<string> AddColumnIfNotExists(List<string> colList, string colName)
        {
            if (!colList.Contains(colName)) colList.Add(colName);

            return colList;
        }

        public bool PrintReport(string strPrinterName, List<string> strParameterArray, string StrReportPath, string StrType)
        {
            try
            {
                string mimtype = "";
                int extension = 1;
                
                var path = Path.Combine(_env.ContentRootPath, "Printer Driver", StrReportPath);
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                parameters.Add("cbNumber", strParameterArray[0]);
                parameters.Add("width", strParameterArray[1]);
                parameters.Add("drop", strParameterArray[2]);

                parameters.Add("customer", strParameterArray[3].ToString());
                parameters.Add("department", strParameterArray[4].ToString());
                parameters.Add("type", strParameterArray[5].ToString());
                parameters.Add("fabric", strParameterArray[6].ToString());
                parameters.Add("color", strParameterArray[7].ToString());
                parameters.Add("controltype", strParameterArray[8].ToString());
                parameters.Add("lathe", strParameterArray[9].ToString());
                
                    parameters.Add("char", strParameterArray[10]);
                    parameters.Add("cutwidth", strParameterArray[14]);
                    parameters.Add("lineNumber", strParameterArray[15].ToString());
                    parameters.Add("cntrside", strParameterArray[16]);
                
                
                parameters.Add("someoftotal", strParameterArray[12] + " of " + strParameterArray[13].ToString());
                LocalReport localReport = new LocalReport(path);
                var result = localReport.Execute(RenderType.Image, extension, parameters, mimtype);

                var outputPath = Path.Combine(_env.ContentRootPath, "Printer Driver", "LogCutPrintFiles", Guid.NewGuid().ToString() + ".png");
                using (FileStream stream = new FileStream(outputPath, FileMode.Create))
                {
                    stream.Write(result.MainStream, 0, result.MainStream.Length);
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
                    pd.Dispose();
                }
                catch (Exception ex)
                {
                    printErrorMessage = "Printing Error: " + ex.ToString();
                    printedOK = false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
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
            catch (Exception)
            {

                return false;
            }

        }


    }
}
