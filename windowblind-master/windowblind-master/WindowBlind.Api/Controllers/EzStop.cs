using Microsoft.AspNetCore.Hosting;
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
using System.Xml.Serialization;
using WindowBlind.Api.Models;

namespace WindowBlind.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EzStop : ControllerBase
    {
        public EzStop(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;


        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails(string CBNumberOrLineNumber)
        {
            try
            {
                bool CBSearch = false;
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
                var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "Deduction");
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

                var SelectedColumnsSetting = await _repository.Settings.FindAsync(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "EzStop");
                var SelectedColumnsPath = SelectedColumnsSetting.FirstOrDefault().settingPath.Split("@@@").ToList();

                if (SelectedColumnsPath.Count == 1 && SelectedColumnsPath[0].Trim() == "") SelectedColumnsPath = new List<string>();

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
                Dictionary<string, int> FixingValues = new Dictionary<string, int>();
                Dictionary<string, List<string>> LatheType = new Dictionary<string, List<string>>();
                List<DropTableModel> Droptable = new List<DropTableModel>();
                List<string> FabricDatable = new List<string>();

                FBRPath = CreateNewFile(FBRPath, FBRPath.Substring(0, FBRPath.IndexOf(".")) + Guid.NewGuid().ToString() + FBRPath.Substring(FBRPath.IndexOf(".")));

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

                System.IO.File.Delete(FBRPath);

                DeductionPath = CreateNewFile(DeductionPath, DeductionPath.Substring(0, DeductionPath.IndexOf(".")) + Guid.NewGuid().ToString() + DeductionPath.Substring(DeductionPath.IndexOf(".")));

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

                System.IO.File.Delete(DeductionPath);

                LathePath = CreateNewFile(LathePath, LathePath.Substring(0, LathePath.IndexOf(".")) + Guid.NewGuid().ToString() + LathePath.Substring(LathePath.IndexOf(".")));

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

                System.IO.File.Delete(LathePath);

                FabricPath = CreateNewFile(FabricPath, FabricPath.Substring(0, FabricPath.IndexOf(".")) + Guid.NewGuid().ToString() + FabricPath.Substring(FabricPath.IndexOf(".")));

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

                System.IO.File.Delete(FabricPath);

                DropPath = CreateNewFile(DropPath, DropPath.Substring(0, DropPath.IndexOf(".")) + Guid.NewGuid().ToString() + DropPath.Substring(DropPath.IndexOf(".")));

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

                System.IO.File.Delete(DropPath);

                var TotalQty = 0;
                ctbsodumpPath = CreateNewFile(ctbsodumpPath, ctbsodumpPath.Substring(0, ctbsodumpPath.IndexOf(".")) + Guid.NewGuid().ToString() + ctbsodumpPath.Substring(ctbsodumpPath.IndexOf(".")));

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
                        bool takeRowCqty = true;
                        for (int j = start.Column; j < end.Column; j++)
                        {
                            var Headertext = worksheet.Cells[1, j].Text.Trim();
                            Headertext = Headertext.Replace(".", "");
                            /// special check 
                            if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "") takeRowCqty = false;
                            var cell = worksheet.Cells[i, j].Text.Trim();

                            if (Headertext.Contains("Qty") && cell != "" && takeRowCqty)
                            {
                                TotalQty += int.Parse(cell);
                            }
                            if (!Data.ColumnNames.Contains(Headertext) && SelectedColumnsPath.Contains(worksheet.Cells[1, j].Text.Trim()))
                                Data.ColumnNames.Add(Headertext);
                            row[Headertext] = cell;

                        }

                        FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                        TblRow.Row = row;
                        TblRow.UniqueId = Guid.NewGuid().ToString();
                        Data.Rows.Add(TblRow);
                    }
                    package.Dispose();
                }
                System.IO.File.Delete(ctbsodumpPath);


                #endregion



                #region Customization of columns

                var f = 0;
                var a = 0;

                var rowCntr = -1;
                foreach (var item in Data.Rows)
                {
                    rowCntr++;
                    item.Row["CB Number"] = item.Row["W/Order NO"].Trim();

                    // WILL ADD CHAR ABCD FOR ITEM NUMBER

                    a = 1;
                    f = 1;
                    for (int j = 0; j < Data.Rows.Count; j++)
                    {
                        if (Data.Rows[j].Row["Line No"] == item.Row["Line No"])
                        {
                            item.Row["item"] = LogCut.CalculateAlphabeticFromNumber(f); break;
                        }
                        f = f + 1;
                        a = a + 1;
                    }

                    item.Row["Total"] = TotalQty.ToString();



                    if (item.Row["Drop"] != String.Empty && int.TryParse(item.Row["Drop"].Trim(), out int res))
                    {
                        var val = Droptable.Where(d => int.Parse(item.Row["Drop"].Trim()) >= d.From && int.Parse(item.Row["Drop"].Trim()) <= d.To).FirstOrDefault();
                        if (val != null)
                        {
                            item.Row["DropGroup"] = val.DropGroup;
                            item.Row["DropColour"] = val.DropColour;
                        }

                    }
                    item.Row["CutWidth"] = GetCutwidth(item.Row["Width"], item.Row["Bind Type/# Panels/Rope/Operation"], item.Row["Fixing Type / Bracket Type"], FixingValues, ControlTypevalues);

                    item.Row["Width"] = item.Row["Width"].Replace("mm", "");
                    item.Row["CutWidth"] = item.Row["CutWidth"].Replace("mm", "");

                    if (item.Row["Width"] != "")
                        item.Row["Width"] = item.Row["Width"] + "mm";
                    else
                        item.Row["Width"] = "0";

                    item.Row["CutWidth_hidden"] = item.Row["CutWidth"];
                    if (item.Row["CutWidth"] != String.Empty)
                        item.Row["CutWidth"] = item.Row["CutWidth"] + "mm";
                    else
                        item.Row["CutWidth"] = "0";


                    item.Row["Qty"] = item.Row["Qty"].Trim();
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

                    if (item.Row["Pull Type / Control Type /Draw Type"] == "OVAL ALUMINIUM BOTTOM BAR")

                        item.Row["Colour"] = item.Row["Pull Colour/Bottom Weight/Wand Len"];
                    else
                    {
                        if (FabricDatable.Contains(item.Row["Fabric"]))
                            item.Row["Colour"] = "PVC Lathe";
                        else
                            item.Row["Colour"] = "Lathe";
                    }
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

                    item.Row["Barcode"] = item.Row["Line No"];

                    if (item.Row["Fabric"].Length > 6)
                        item.Row["Fabric"] = item.Row["Fabric"].Substring(0, item.Row["Fabric"].Length - 6);
                    else
                        item.Row["Fabric"] = item.Row["Fabric"];


                    item.Row["Date-Time"] = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                }

                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "CB Number");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "item");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Qty");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Width");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "CutWidth");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Tube");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Spring");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Finish");
                Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Colour");
                #endregion
                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }


        private string GetCutwidth(string width, string value, string fixing, Dictionary<string, int> FixingValues, Dictionary<string, int> ControlTypevalues)
        {
            if (width == String.Empty || int.TryParse(width, out int res) == false)
                return "";
            else
            {
                if (int.Parse(width) == 0)
                    return width;

            }

            if (value.Trim() == String.Empty)
                value = "BLANK";


            var DeductedValue = 0;
            var deductedFixedValue = 0;
            if (ControlTypevalues.ContainsKey(value.Trim()))
                DeductedValue = ControlTypevalues[value.Trim()];

            if (FixingValues.ContainsKey(value.Trim()))
                deductedFixedValue = FixingValues[value.Trim()];


            string cutwidth = (Double.Parse(width) - DeductedValue - deductedFixedValue).ToString();


            return cutwidth;
        }


        [HttpGet("RefreshEzStopTable")]
        public async Task<IActionResult> RefreshEzStopTable()
        {

            try
            {
                var XMLSetting = await _repository.Settings.FindAsync(e => e.settingName == "XML File");
                var XMLPath = XMLSetting.FirstOrDefault().settingPath;
                FileInfo file = new FileInfo(XMLPath);
                if (!file.Exists) new JsonResult(false);

                XmlSerializer reader =
                   new XmlSerializer(typeof(OrderObject));
                System.IO.StreamReader file32 = new StreamReader(XMLPath);
                var result = (OrderObject)reader.Deserialize(file32);
                file32.Close();

                return await getCBNumberDetails(result.OrderNumber);
            }
            catch (Exception)
            {

                return new JsonResult(false);
            }

        }


        [HttpPost("EzStopSend")]
        public async Task<IActionResult> EzStopSend(CreateFileAndLabelModel model)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var EzStopOutputSetting = await _repository.Settings.FindAsync(e => e.settingName == "EzStop Output");
                var EzStopFilePath = EzStopOutputSetting.FirstOrDefault().settingPath;
                if (EzStopFilePath == "") return new JsonResult(false);
                DirectoryInfo f = new DirectoryInfo(EzStopFilePath);

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
                    strconcat += "@" + item.Row["Fabric"] + "@" + item.Row["Control Type"] + "@" + item.Row["Colour"];
                    strconcat += "@" + item.Row["Lathe"];
                    strconcat += "@" + item.Row["Alpha"] + "@" + item.Row["CB Number"];
                    strconcat += "@" + item.Row["SRLineNumber"];

                    strconcat += "@" + item.Row["Total"];
                    labels.Add(strconcat);
                    strRS232Width += item.Row["CutWidth"].ToString().Replace("mm", "");
                    await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), model.tableName, model.userName, System.DateTime.Now.ToString(), item.Row["Alpha"], "EzStop", item);
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
                    applicationtype = "EzStop"
                };

                await _repository.comport.InsertOneAsync(comport);

                // add to file 
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Ezystop");

                List<string> ExcelFileColumnNames = new List<string>
                {
                    "CB Number",
                    "item",
                    "Qty",
                    "Width",
                    "CutWidth",
                    "Tube",
                    "Date-Time",
                    "Spring",
                    "Finish",
                    "Colour",
                    "Line No"

                };


                wsSheet1.Cells["A1:I1"].Merge = true;

                wsSheet1.Cells[1, 1].Value = "Ezystop";
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
                foreach (var col in ExcelFileColumnNames)
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
                    foreach (var col in ExcelFileColumnNames)
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
                EzStopFilePath = Path.Combine(EzStopFilePath, "EzStopOutput" + DateTime.Now.ToString("dd-mm-yyyy  hh-mm-ss") + ".xlsx");
                ExcelPkg.SaveAs(new FileInfo(EzStopFilePath));

                // the printing

                for (int k = 0; k < labels.Count; k++)
                {
                    var strParameterArray = labels[k].ToString().Split("@");
                    var strpara = strParameterArray;


                    if (strpara.Length == 0) continue;

                    var ret1 = PrintReport("EzStop.rdlc", printerName, strParameterArray.ToList());

                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {
                return new JsonResult(false);
            }
        }

        public bool PrintReport(string StrReportPath, string strPrinterName, List<string> strParameterArray)
        {
            try
            {
                string mimtype = "";
                int extension = 1;
                var path = Path.Combine(_env.ContentRootPath, "Printer Driver", StrReportPath);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");
                var parametersList = new List<ReportParameter>();

                parametersList.Add(new ReportParameter("someoftotal", strParameterArray[11] + " of " + strParameterArray[12].ToString()));
                parametersList.Add(new ReportParameter("char", strParameterArray[9].ToString()));
                parametersList.Add(new ReportParameter("lathe", strParameterArray[8].ToString()));
                parametersList.Add(new ReportParameter("controltype", strParameterArray[6].ToString()));
                parametersList.Add(new ReportParameter("color", strParameterArray[7].ToString()));
                parametersList.Add(new ReportParameter("fabric", strParameterArray[5].ToString()));
                //parametersList.Add(new ReportParameter("type", strParameterArray[5].ToString()));
                parametersList.Add(new ReportParameter("department", strParameterArray[4].ToString()));
                parametersList.Add(new ReportParameter("customer", strParameterArray[3].ToString()));
                parametersList.Add(new ReportParameter("drop", strParameterArray[2].ToString()));
                parametersList.Add(new ReportParameter("width", strParameterArray[1].ToString()));
                parametersList.Add(new ReportParameter("cbNumber", strParameterArray[0].ToString()));

                LocalReport report = new LocalReport();
                report.ReportPath = path;
                report.SetParameters(parametersList);
                report.Refresh();
                byte[] result = report.Render("IMAGE");
                report.Dispose();

                var outputPath = Path.Combine(_env.ContentRootPath, "Printer Driver", "EzStopPrintFiles", Guid.NewGuid().ToString() + ".png");
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
                log.TableName = tableNo;
                await _repository.Logs.InsertOneAsync(log);
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }

        public string CreateNewFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
            return destination;

        }


    }
}
