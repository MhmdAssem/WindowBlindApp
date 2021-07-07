using Microsoft.AspNetCore.Hosting;
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
                                if (Data.Rows[j].Row["Line No."] == item.Row["Line No."])
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
                            item.Row["DropColour"] = val.DropColour;
                        }

                    }

                    if (item.Row["Width"] != "")
                        item.Row["Width"] = item.Row["Width"] + "mm";
                    else
                        item.Row["Width"] = "0";

                    item.Row["CutWidth"] = GetCutwidth2(item.Row["Width"], item.Row["Bind. Type/# Panels/Rope/Operation"], ControlTypevalues);
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

                    if (item.Row["Bind. Type/# Panels/Rope/Operation"].ToString().Trim() == "SPRING" || item.Row["Bind. Type/# Panels/Rope/Operation"].ToString().Trim() == "SPRINGHD")
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
                                    blindNumber++;
                                    a += 1;
                                    FinalRow.Row["SRLineNumber"] = a.ToString();
                                    FinalizedData.Rows.Add(FinalRow);

                                }
                            }
                            else
                            {
                                if (item.Row["Line No."] == LineNumber)
                                {
                                    var Quantity = int.Parse(item.Row["Qty"]);
                                    item.Row["Qty"] = "1";
                                    for (int j = 0; j < Quantity; j++)
                                    {
                                        var FinalRow = new FabricCutterCBDetailsModelTableRow();
                                        FinalRow = item;
                                        FinalRow.Row["Blind Number"] = (blindNumber).ToString();
                                        blindNumber++;
                                        a += 1;
                                        FinalRow.Row["SRLineNumber"] = a.ToString();
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
                FinalizedData.ColumnNames = AddColumnIfNotExists(FinalizedData.ColumnNames, "Line No.");

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
                FileInfo f = new FileInfo(LogCutOutputPath);

                if (!f.Exists && !LogCutOutputPath.EndsWith(".xslx")) return new JsonResult(false);

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
                    strconcat += "@" + item.Row["Line No."];
                    strconcat += "@" + item.Row["ControlSide"];
                    labels.Add(strconcat);
                    strRS232Width += item.Row["CutWidth"].ToString().Replace("mm", "");
                }
                // doing the port thing 

                // add to file 
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                /// get old style
                ExcelPackage ExcelPkgOld = new ExcelPackage(f);
                ExcelWorksheet wsSheet1Old = ExcelPkgOld.Workbook.Worksheets.Where(e => e.Name == "Logcut").FirstOrDefault();



                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Logcut");


                wsSheet1.Cells["A1:I1"].Merge = true;

                wsSheet1.Cells[1, 1].Value = "Logcut";
                wsSheet1.Cells[1, 1].Style.Font = wsSheet1Old.Cells[1, 1].Style.Font;
                wsSheet1.Cells[1, 1].Style.Font.Size = wsSheet1Old.Cells[1, 1].Style.Font.Size;
                wsSheet1.Cells[1, 1].Style.Font.Italic = wsSheet1Old.Cells[1, 1].Style.Font.Italic;
                wsSheet1.Cells[1, 1].Style.Font.UnderLine = wsSheet1Old.Cells[1, 1].Style.Font.UnderLine;
                wsSheet1.Cells[1, 1].Style.Font.UnderLineType = wsSheet1Old.Cells[1, 1].Style.Font.UnderLineType;
                wsSheet1.Cells[1, 1].Style.Font.Name = wsSheet1Old.Cells[1, 1].Style.Font.Name;
                wsSheet1.Cells[1, 1].Style.Font.Bold = wsSheet1Old.Cells[1, 1].Style.Font.Bold;
                wsSheet1.Cells[1, 1].Style.HorizontalAlignment = wsSheet1Old.Cells[1, 1].Style.HorizontalAlignment;
                wsSheet1.Cells[1, 1].Style.VerticalAlignment = wsSheet1Old.Cells[1, 1].Style.VerticalAlignment;
                wsSheet1.Cells[1, 1].Style.Border = wsSheet1Old.Cells[1, 1].Style.Border;

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

                f.Delete();
                ExcelPkg.SaveAs(new FileInfo(LogCutOutputPath));

                // the printing

                for (int k = 0; k < labels.Count; k++)
                {
                    var strParameterArray = labels[k].ToString().Split("@");
                    var strpara = strParameterArray;


                    if (strpara.Length == 0) continue;

                    var ret1 = PrintReport("1", printerName, strParameterArray.ToList(), "PrintLabel.rpt", "Width");
                    var ret2 = PrintReport("1", printerName, strParameterArray.ToList(), "PrintLabelCutWidth.rpt", "CutWidth");

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


        private bool PrintReport(string strNoCopy, string strPrinterName, List<string> strParameterArray, string StrReportPath, string StrType)
        {
            try
            {
                //CrystalDecisions.Shared.ConnectionInfo crDbConnection = new CrystalDecisions.Shared.ConnectionInfo();

                //CrystalDecisions.CrystalReports.Engine.ReportDocument oRpt = new CrystalDecisions.CrystalReports.Engine.ReportDocument();

                //System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();

                //System.Drawing.Printing.PaperSize pkSize = new System.Drawing.Printing.PaperSize();



                //oRpt.FileName = Path.Combine(_env.ContentRootPath, StrReportPath);
                //oRpt.SetParameterValue("@CBNumber", strParameterArray[0]);
                //oRpt.SetParameterValue("@Width", strParameterArray[1]);
                //oRpt.SetParameterValue("@Drop", strParameterArray[2]);
                //if (strParameterArray[3].Length > 20)
                //    oRpt.SetParameterValue("@Customer", strParameterArray[3].Substring(0, 20));
                //else
                //    oRpt.SetParameterValue("@Customer", strParameterArray[3]);


                //if (strParameterArray[4].Length > 10)
                //    oRpt.SetParameterValue("@Department", strParameterArray[4].Substring(0, 10));
                //else
                //    oRpt.SetParameterValue("@Department", strParameterArray[4]);


                //oRpt.SetParameterValue("@Type", strParameterArray[5]);

                //if (strParameterArray[6].Length > 12)
                //    oRpt.SetParameterValue("@Fabric", strParameterArray[6].Substring(0, 12));
                //else
                //    oRpt.SetParameterValue("@Fabric", strParameterArray[6]);


                //if (strParameterArray[7].Length > 12)
                //    oRpt.SetParameterValue("@Color", strParameterArray[7].Substring(0, 12));

                //oRpt.SetParameterValue("@Color", strParameterArray[7]);


                //if (strParameterArray[8].Length > 6)
                //    oRpt.SetParameterValue("@ControlType", strParameterArray[8].Substring(0, 6));
                //else
                //    oRpt.SetParameterValue("@ControlType", strParameterArray[8]);


                //oRpt.SetParameterValue("@Lathe", strParameterArray[9]);
                //oRpt.SetParameterValue("@Alpha", strParameterArray[10]);
                //oRpt.SetParameterValue("@Barcode", strParameterArray[11]);
                //oRpt.SetParameterValue("@strLineNumber", strParameterArray[12]);

                //oRpt.SetParameterValue("@Total", strParameterArray[13]);
                //if (StrType.ToUpper() == "CUTWIDTH")
                //{
                //    oRpt.SetParameterValue("@CutWidth", strParameterArray[14]);
                //    oRpt.SetParameterValue("@ControlSide", strParameterArray[16]);
                //}

                //oRpt.SetParameterValue("@LineNo", strParameterArray[15]);

                //oRpt.PrintOptions.PrinterName = strPrinterName;


                //for (int i = 1; i < (strNoCopy).Length; i++)
                //{
                //    try
                //    {
                //        oRpt.PrintToPrinter(1, false, 0, 0);

                //    }
                //    catch (Exception e)
                //    {

                //        return false;
                //    }
                //}
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
