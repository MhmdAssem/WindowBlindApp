
using AspNetCore.Reporting;
using Microsoft.AspNetCore.Components;
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
using System.Threading;
using System.Threading.Tasks;
using WindowBlind.Api.Models;
using PrstringerProject;


namespace WindowBlind.Api.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
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

        Dictionary<string, int> ColumnsIndex;
        string ctbsodumpPath, SheetNamePath, FBRPath, DeductionPath, LathePath, FabricPath, DropPath;
        int TotalQty;
        List<string> SelectedColumnsNames;

        Dictionary<string, string> FabricRollwidth;
        Dictionary<string, int> ControlTypevalues;
        Dictionary<string, List<string>> LatheType;
        List<DropTableModel> Droptable;
        List<string> FabricDatable;
        bool CBSearch;
        string CB;
        string LineNumber;
        Dictionary<string, string> ColumnMapper = new Dictionary<string, string>();


        public void Init()
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

            /// get Fabric Roll WIdth File

            var FBRSetting = await _repository.Settings.FindAsync(e => e.settingName == "Fabric Rollwidth");
            FBRPath = FBRSetting.FirstOrDefault().settingPath;

            /// get deduct width file
            var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "DeductionTable");
            DeductionPath = DeductionSetting.FirstOrDefault().settingPath;

            ///get Lathe file path
            var LatheSetting = await _repository.Settings.FindAsync(e => e.settingName == "PVCLathe Fabric");
            LathePath = LatheSetting.FirstOrDefault().settingPath;

            /// get the Fabric data 
            var FabricSetting = await _repository.Settings.FindAsync(e => e.settingName == "FabricTable");
            FabricPath = FabricSetting.FirstOrDefault().settingPath;

            /// get the Fabric data 
            var DropSetting = await _repository.Settings.FindAsync(e => e.settingName == "DropTable");
            DropPath = DropSetting.FirstOrDefault().settingPath;

            var SelectedColumnsSetting = await _repository.Settings.FindAsync(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "LogCut");
            SelectedColumnsNames = SelectedColumnsSetting.FirstOrDefault().settingPath.Split("@@@").ToList();

            if (SelectedColumnsNames.Count == 1 && SelectedColumnsNames[0].Trim() == "") SelectedColumnsNames = new List<string>();


        }

        private bool CheckPaths()
        {
            FileInfo file = new FileInfo(ctbsodumpPath);
            if (!file.Exists) return false;


            file = new FileInfo(FBRPath);
            if (!file.Exists) return false;


            file = new FileInfo(DeductionPath);
            if (!file.Exists) return false;


            file = new FileInfo(LathePath);
            if (!file.Exists) return false;

            file = new FileInfo(FabricPath);
            if (!file.Exists) return false;

            file = new FileInfo(DropPath);
            if (!file.Exists) return false;

            return true;
        }

        private FabricCutterCBDetailsModel ReadingData()
        {
            FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
            FileInfo file = new FileInfo(ctbsodumpPath);
            if (!file.Exists) return null;
            List<string> names = new List<string>();
            //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();



            FabricRollwidth = new Dictionary<string, string>();
            ControlTypevalues = new Dictionary<string, int>();
            LatheType = new Dictionary<string, List<string>>();
            Droptable = new List<DropTableModel>();
            FabricDatable = new List<string>();

            var TempFBRPath = CreateNewFile(FBRPath, FBRPath.Substring(0, FBRPath.IndexOf(".")) + Guid.NewGuid().ToString() + FBRPath.Substring(FBRPath.IndexOf(".")));

            file = new FileInfo(TempFBRPath);
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

            var TempFabricPath = CreateNewFile(FabricPath, FabricPath.Substring(0, FabricPath.IndexOf(".")) + Guid.NewGuid().ToString() + FabricPath.Substring(FabricPath.IndexOf(".")));

            file = new FileInfo(TempFabricPath);
            using (var package = new ExcelPackage(file))
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.FirstOrDefault();

                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;
                for (int i = start.Row + 1; i <= end.Row; i++)
                {
                    var text = worksheet.Cells[i, 1].Text.Trim();
                    if (!FabricDatable.Contains(text))
                        FabricDatable.Add(text);
                }
            }

            System.IO.File.Delete(TempFabricPath);

            var TempDropPath = CreateNewFile(DropPath, DropPath.Substring(0, DropPath.IndexOf(".")) + Guid.NewGuid().ToString() + DropPath.Substring(DropPath.IndexOf(".")));

            file = new FileInfo(TempDropPath);
            using (var package = new ExcelPackage(file))
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.FirstOrDefault();

                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;
                for (int i = start.Row + 1; i <= end.Row; i++)
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

            System.IO.File.Delete(TempDropPath);

            TotalQty = 0;
            var TempctbsodumpPath = CreateNewFile(ctbsodumpPath, ctbsodumpPath.Substring(0, ctbsodumpPath.IndexOf(".")) + Guid.NewGuid().ToString() + ctbsodumpPath.Substring(ctbsodumpPath.IndexOf(".")));

            file = new FileInfo(TempctbsodumpPath);
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
                    var CBINdex = 0;
                    for (int i = start.Column; i <= end.Column; i++)
                    {
                        var text = worksheet.Cells[1, i].Text.Trim();

                        if (text.Equals("W/Order NO")) CBINdex = i;
                        if (text.Equals("Line No."))
                        {
                            for (int j = start.Row + 1; j <= end.Row; j++)
                                if (worksheet.Cells[j, i].Text.Trim() == LineNumber) { CB = worksheet.Cells[j, CBINdex].Text.Trim(); break; }
                        }
                    }
                }

                for (int i = start.Column; i <= end.Column; i++)
                {
                    ColumnsIndex[worksheet.Cells[1, i].Text.Trim()] = i;
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    if (Headertext.StartsWith("W/Order NO"))
                    {
                        for (int j = start.Row + 1; j <= end.Row; j++)
                            if (worksheet.Cells[j, i].Text.Trim() != CB) indexToRemove[j] = 1;

                    }
                }

                for (int i = start.Row + 1; i <= end.Row; i++)
                {
                    if (indexToRemove.ContainsKey(i)) continue;
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    int RowQty = 0;
                    for (int j = start.Column; j <= end.Column; j++)
                    {
                        var Headertext = worksheet.Cells[1, j].Text.Trim();
                        if (String.IsNullOrEmpty(Headertext)) continue;

                        Headertext = Headertext.Replace(".", "");
                        /// special check 
                        if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "")
                        {
                            RowQty = 0; break;
                        };
                        var cell = worksheet.Cells[i, j].Text.Trim();

                        if (Headertext.Contains("Qty") && cell != "")
                        {
                            RowQty = int.Parse(cell);
                            TotalQty += int.Parse(cell);
                        }
                        if (!Data.ColumnNames.Contains(Headertext) && SelectedColumnsNames.Contains(worksheet.Cells[1, j].Text.Trim()))
                            Data.ColumnNames.Add(Headertext);

                        if (ColumnMapper.ContainsKey(Headertext))
                        {
                            row[Headertext] = cell;
                            Headertext = ColumnMapper[Headertext];
                        }
                        row[Headertext] = cell;

                    }
                    if (RowQty == 0) continue;
                    FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                    TblRow.Row = row;
                    TblRow.UniqueId = Guid.NewGuid().ToString();
                    TblRow.rows_AssociatedIds.Add(TblRow.UniqueId);
                    Data.Rows.Add(TblRow);
                }
                package.Dispose();
            }

            System.IO.File.Delete(TempctbsodumpPath);
            return Data;
        }

        private void LogCutProcess(ref FabricCutterCBDetailsModel FinalizedData, ref FabricCutterCBDetailsModel Data)
        {

            var f = 0;
            var a = 0;
            var Fbindex = 0;
            var blindNumber = 1;

            var CurrentCBNumber = "";
            foreach (var item in Data.Rows)
            {
                if (item.Row["CB Number"] != CurrentCBNumber)
                {
                    CurrentCBNumber = item.Row["CB Number"];
                    f = 0;
                    a = 0;
                    Fbindex = 0;
                    blindNumber = 1;

                }
                Fbindex = 0;

                if (item.Row.ContainsKey("Fabric") && item.Row["Fabric"] != "")
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

                item.Row["CutWidth"] = GetCutwidth2(item.Row["Width"], item.Row["Bind Type/# Panels/Rope/Operation"], ControlTypevalues);

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
                    else
                    {
                        item.Row["Finish"] = item.Row["Description"].ToString().Trim(); /// check with dhaval
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
                                var FinalRow = new FabricCutterCBDetailsModelTableRow
                                {
                                    FileName = item.FileName,
                                    BlindNumbers = item.BlindNumbers,
                                    CreationDate = item.CreationDate,
                                    PackingType = item.PackingType,
                                    Row = item.Row,
                                    rows_AssociatedIds = new List<string>(),
                                    UniqueId = ""
                                };

                                FinalRow.Row["Blind Number"] = (blindNumber).ToString();
                                FinalRow.Row["SRLineNumber"] = blindNumber.ToString();
                                FinalRow.Row["FromHoldingStation"] = "NO";
                                FinalRow.UniqueId = Guid.NewGuid().ToString();

                                blindNumber++;
                                a += 1;
                                FinalRow.rows_AssociatedIds.Add(FinalRow.UniqueId);
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

                                    var FinalRow = new FabricCutterCBDetailsModelTableRow
                                    {
                                        FileName = item.FileName,
                                        BlindNumbers = item.BlindNumbers,
                                        CreationDate = item.CreationDate,
                                        PackingType = item.PackingType,
                                        Row = item.Row,
                                        rows_AssociatedIds = new List<string>(),
                                        UniqueId = ""
                                    };
                                    FinalRow.Row["Blind Number"] = (blindNumber).ToString();
                                    FinalRow.Row["SRLineNumber"] = blindNumber.ToString();
                                    FinalRow.Row["FromHoldingStation"] = "NO";
                                    FinalRow.UniqueId = Guid.NewGuid().ToString();
                                    FinalRow.rows_AssociatedIds.Add(FinalRow.UniqueId);

                                    blindNumber++;
                                    a += 1;

                                    FinalizedData.Rows.Add(FinalRow);
                                }
                            }
                        }
                    }
                }
            }

        }

        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails([FromHeader] string CBNumberOrLineNumber)
        {
            try
            {

                CBSearch = !char.IsDigit(CBNumberOrLineNumber[0]);
                CB = CBNumberOrLineNumber;
                LineNumber = CBNumberOrLineNumber;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ColumnsIndex = new Dictionary<string, int>();

                Init();
                await ReadConfig();
                var Checks = CheckPaths();

                if (!Checks) return new JsonResult(false);

                var Data = ReadingData();

                FabricCutterCBDetailsModel FinalizedData = new FabricCutterCBDetailsModel();

                LogCutProcess(ref FinalizedData, ref Data);

                CustomizeTheColumns(ref FinalizedData);

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
                var LogCutOutputSettings = await _repository.Tables.FindAsync(e => e.TableName == model.tableName);
                var LogCutOutputPath = LogCutOutputSettings.FirstOrDefault().OutputPath;
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
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });

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

                    bool res = await insertLog(item.Row["CB Number"].ToString().TrimEnd(), item.Row["Barcode"].ToString().TrimEnd(), model.tableName, model.userName, System.DateTime.Now.ToString(), item.Row["Alpha"], "LogCut", item);

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

                ExcelPkg.SaveAs(new FileInfo(LogCutOutputPath));

                // the printing

                for (int k = 0; k < labels.Count; k++)
                {
                    var strParameterArray = labels[k].ToString().Split("@");

                    PrintReport(printerName, strParameterArray.ToList(), "LogCut1.rdlc", "Width");
                    PrintReport(printerName, strParameterArray.ToList(), "LogCut2.rdlc", "");


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
        //private Stream CreateStream(string name,
        //    string fileNameExtension, Encoding encoding,
        //    string mimeType, bool willSeek)
        //{
        //    Stream stream = new MemoryStream();
        //    m_streams.Add(stream);
        //    return stream;
        //}
        public bool PrintReport(string strPrinterName, List<string> strParameterArray, string StrReportPath, string StrType)
        {
            try
            {
                string mimtype = "";
                int extension = 1;

                var path = Path.Combine("E:\\Webapp_input files", "Printer Driver", StrReportPath);
                //var path = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject", StrReportPath);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");
                var parametersList = new Dictionary<string, string>();
                //var parametersList = new List<ReportParameter>();


                for (int i = 0; i < strParameterArray.Count; i++)
                {
                    while (strParameterArray[i].IndexOf("  ") != -1)
                        strParameterArray[i] = strParameterArray[i].Replace("  ", " ");
                    if (String.IsNullOrEmpty(strParameterArray[i]))
                        strParameterArray[i] = " ";
                }

                parametersList.Add("width", strParameterArray[1].ToString());
                parametersList.Add("drop", strParameterArray[2].ToString());
                parametersList.Add("customer", strParameterArray[3].ToString());
                parametersList.Add("department", strParameterArray[4].ToString());
                parametersList.Add("type", strParameterArray[5].ToString());
                parametersList.Add("fabric", strParameterArray[6].ToString());
                parametersList.Add("color", strParameterArray[7].ToString());
                parametersList.Add("controltype", strParameterArray[8].ToString());
                parametersList.Add("lathe", strParameterArray[9].ToString());
                parametersList.Add("char", strParameterArray[10]);
                parametersList.Add("cbNumber", strParameterArray[0].ToString());
                parametersList.Add("someoftotal", strParameterArray[12] + " of " + strParameterArray[13].ToString());


                if (StrType != "")
                {
                    parametersList.Add("cutwidth", strParameterArray[14]);
                    parametersList.Add("lineNumber", strParameterArray[15]);
                    parametersList.Add("cntrside", strParameterArray[16]);
                }



                AspNetCore.Reporting.LocalReport report = new AspNetCore.Reporting.LocalReport(path);

                if (StrReportPath == "LogCut2.rdlc")
                {
                    LogCut2 obj = new LogCut2();
                    obj.width = strParameterArray[1].ToString();
                    obj.drop = strParameterArray[2].ToString();
                    obj.customer = strParameterArray[3].ToString();
                    obj.department = strParameterArray[4].ToString();
                    obj.type = strParameterArray[5].ToString();
                    obj.fabric = strParameterArray[6].ToString();
                    obj.color = strParameterArray[7].ToString();
                    obj.controltype = strParameterArray[8].ToString();
                    obj.lathe = strParameterArray[9].ToString();
                    obj.c = strParameterArray[10].ToString();
                    obj.cbNumber = strParameterArray[0].ToString();
                    obj.someoftotal = strParameterArray[12] + " of " + strParameterArray[13].ToString();

                    List<LogCut2> ls = new List<LogCut2> {
                    obj
                    };
                    report.AddDataSource("LogCut2", ls);
                    parametersList = null;

                }
                   



                byte[] result = report.Execute(RenderType.Image, extension, parametersList, mimtype).MainStream;

                /*LocalReport report = new LocalReport();
                report.ReportPath = path;
                report.SetParameters(parametersList);
                report.Refresh();
                byte[] result = report.Render("IMAGE");
                report.Dispose();
                */

                var outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "LogCutPrintFiles", Guid.NewGuid().ToString() + ".jpg");
                //var outputPath = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject\\Delete", Guid.NewGuid().ToString() + ".png");
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
                        m.Y = 0;//(int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Height - m.Height) / 2);
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

        public string CreateNewFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
            return destination;

        }


        [HttpGet("GetHeldObjects")]
        public async Task<Object> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();

                CBSearch = true;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ColumnsIndex = new Dictionary<string, int>();

                Init();
                await ReadConfig();

                var check = CheckPaths();

                if (!check) return new JsonResult(false);


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "LogCut" && rej.TableName == tableName).Result.ToListAsync();

                Parallel.ForEach(HeldObjects, item =>
                {

                    CB = item.Row.Row["CB Number"];
                    var HeldData = ReadingData();
                    var TempFinalData = new FabricCutterCBDetailsModel();
                    LogCutProcess(ref TempFinalData, ref HeldData);


                    Data.Rows.Add(HeldData.Rows.Where(e => e.Row["Line No"] == item.Row.Row["Line No"]).FirstOrDefault());
                    Data.Rows[Data.Rows.Count - 1].Row["FromHoldingStation"] = "YES";
                    Data.Rows[Data.Rows.Count - 1].UniqueId = item.Row.UniqueId;
                    Data.ColumnNames = TempFinalData.ColumnNames;

                });

                return Data;

            }
            catch (Exception e)
            {

                return new FabricCutterCBDetailsModel();
            }
        }

        [HttpPost("ClearOrdersFromLogCut")]
        public async Task<IActionResult> ClearOrdersFromLogCut([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                foreach (var item in model.data.Rows)
                {
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });
                }
                return new JsonResult(true);
            }
            catch (Exception e)
            {

                return new JsonResult(e.Message);
            }

        }

        [HttpGet("GetDataUsingAutoUpload")]
        public async Task<IActionResult> GetDataUsingAutoUpload([FromHeader] string TableName, [FromHeader] string UserName, [FromHeader] string Shift, [FromHeader] string Type)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Init();
                FabricCutterCBDetailsModel Finalized = new FabricCutterCBDetailsModel();


                await ReadConfig();
                var check = CheckPaths();

                if (!check) return new JsonResult(false);
                var AutoUploadDirSetting = await _repository.Settings.FindAsync(e => e.settingName == "AutoUploadDir" && e.applicationSetting == "_LogCut");
                var AutoUploadDirPath = AutoUploadDirSetting.FirstOrDefault().settingPath;

                var ViewedUploadsSetting = await _repository.Settings.FindAsync(e => e.settingName == "ViewedUploadsDir" && e.applicationSetting == "_LogCut");
                var ViewedUplaodsPath = ViewedUploadsSetting.FirstOrDefault().settingPath;

                var AutoUploadFolder = new DirectoryInfo(AutoUploadDirPath);
                if (AutoUploadFolder.Exists == false)
                    return new JsonResult(false);

                var AutoUploadErrorsPath = ViewedUplaodsPath.Substring(0, ViewedUplaodsPath.LastIndexOf(Path.DirectorySeparatorChar.ToString())) + Path.DirectorySeparatorChar.ToString() + "AutoUploadErrorFolder";

                var ErrorDirectory = new DirectoryInfo(AutoUploadErrorsPath);
                if (!ErrorDirectory.Exists)
                    ErrorDirectory.Create();

                CBSearch = true;

                #region Reading Data

                /// first Get all Files that Starts with the TableName

                var files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains(TableName + "_" + Shift) && !file.Name.Contains("Urgent")).ToList();
                if (Type == "Urgent")
                    files = AutoUploadFolder.GetFiles().Where(file => file.Name.Contains("Urgent_" + TableName + "_" + Shift)).ToList();

                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();
                List<string> names = new List<string>();

                FabricRollwidth = new Dictionary<string, string>();
                ControlTypevalues = new Dictionary<string, int>();
                LatheType = new Dictionary<string, List<string>>();
                Droptable = new List<DropTableModel>();
                FabricDatable = new List<string>();

                var TempFBRPath = CreateNewFile(FBRPath, FBRPath.Substring(0, FBRPath.IndexOf(".")) + Guid.NewGuid().ToString() + FBRPath.Substring(FBRPath.IndexOf(".")));

                var file = new FileInfo(TempFBRPath);
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

                var TempFabricPath = CreateNewFile(FabricPath, FabricPath.Substring(0, FabricPath.IndexOf(".")) + Guid.NewGuid().ToString() + FabricPath.Substring(FabricPath.IndexOf(".")));

                file = new FileInfo(TempFabricPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i <= end.Row; i++)
                    {
                        var text = worksheet.Cells[i, 1].Text.Trim();
                        if (!FabricDatable.Contains(text))
                            FabricDatable.Add(text);
                    }
                }

                System.IO.File.Delete(TempFabricPath);

                var TempDropPath = CreateNewFile(DropPath, DropPath.Substring(0, DropPath.IndexOf(".")) + Guid.NewGuid().ToString() + DropPath.Substring(DropPath.IndexOf(".")));

                file = new FileInfo(TempDropPath);
                using (var package = new ExcelPackage(file))
                {
                    var workbook = package.Workbook;

                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;
                    for (int i = start.Row + 1; i <= end.Row; i++)
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

                System.IO.File.Delete(TempDropPath);

                #endregion

                int generalBlindNumber = 1;

                foreach (var AutoUploadFile in files)
                {
                    try /// so if there is error get data from db
                    {
                        using (var package = new ExcelPackage(AutoUploadFile))
                        {
                            var workbook = package.Workbook;

                            var worksheet = workbook.Worksheets.FirstOrDefault();
                            if (worksheet == null) return null;
                            var start = worksheet.Dimension.Start;
                            var end = worksheet.Dimension.End;
                            bool gotColumns = (SelectedColumnsNames.Count > 0) ? true : false;
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

                                    if (ColumnMapper.ContainsKey(Headertext))
                                    {
                                        row[Headertext] = cell;
                                        Headertext = ColumnMapper[Headertext];
                                    }
                                    row[Headertext] = cell;


                                }

                                FabricCutterCBDetailsModelTableRow TblRow = new FabricCutterCBDetailsModelTableRow();
                                for (int cntr = generalBlindNumber; cntr < RowQty + generalBlindNumber; cntr++)
                                {
                                    TblRow.BlindNumbers.Add(cntr);
                                }
                                if (RowQty > 0)
                                {
                                    generalBlindNumber += RowQty;
                                    TblRow.Row = row;
                                    TblRow.UniqueId = Guid.NewGuid().ToString();
                                    TblRow.rows_AssociatedIds.Add(TblRow.UniqueId);
                                    TblRow.CreationDate = AutoUploadFile.CreationTime.ToString();
                                    TblRow.FileName = AutoUploadFile.Name;

                                    Data.Rows.Add(TblRow);
                                }
                            }

                            package.Dispose();
                        }

                        #region Customization of Data

                        LogCutProcess(ref Finalized, ref Data);

                        #endregion

                        #region Moving Already Finished Files to ViewedAutoUpload 

                        FileInfo checking = new FileInfo(Path.Combine(ViewedUplaodsPath, AutoUploadFile.Name));
                        Random rd = new Random();


                        if (checking.Exists)
                            CreateNewFile(AutoUploadFile.FullName, Path.Combine(ViewedUplaodsPath, rd.Next(1, 1000000).ToString() + "_" + AutoUploadFile.Name));
                        else
                            CreateNewFile(AutoUploadFile.FullName, Path.Combine(ViewedUplaodsPath, AutoUploadFile.Name));
                        System.IO.File.Delete(AutoUploadFile.FullName);

                        #endregion

                        #region Adding the Processed Data to the DB

                        foreach (var item in Finalized.Rows)
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
                                Station = "LogCut"
                            };
                            await _repository.AutoUploads.InsertOneAsync(model);
                        }

                        #endregion

                        Finalized.Rows.Clear();
                        Data.Rows.Clear();
                    }
                    catch (Exception e)
                    {

                        #region Moving Files with Errors to the error specified Folder

                        FileInfo checking = new FileInfo(Path.Combine(ViewedUplaodsPath, AutoUploadFile.Name));
                        Random rd = new Random();


                        if (checking.Exists)
                            CreateNewFile(AutoUploadFile.FullName, Path.Combine(AutoUploadErrorsPath, rd.Next(1, 1000000).ToString() + "_" + AutoUploadFile.Name));
                        else
                            CreateNewFile(AutoUploadFile.FullName, Path.Combine(AutoUploadErrorsPath, AutoUploadFile.Name));
                        System.IO.File.Delete(AutoUploadFile.FullName);


                        #endregion


                    }
                }


                #region Reading Data from Auto Upload DB

                var AutoUploadsModels = await _repository.AutoUploads.FindAsync(res => res.TableName == TableName && res.Shift == Shift && res.Type == Type && res.Station == "LogCut").Result.ToListAsync();
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

                    Finalized.Rows.Add(item.row);

                }

                #endregion

                #region getting Held Orders

                if (Type == "Normal")
                {
                    FabricCutterCBDetailsModel newdata = (FabricCutterCBDetailsModel)(await GetHeldObjects(TableName));
                    if (Finalized.Rows != null)
                    {
                        newdata.Rows.AddRange(Finalized.Rows);
                        newdata.ColumnNames = Finalized.ColumnNames;
                        Finalized = newdata;
                    }
                    else
                        Finalized = newdata;
                }
                #endregion

                #region Customization of Columns

                CustomizeTheColumns(ref Finalized);

                #endregion


                return new JsonResult(Finalized);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);

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

        private void CustomizeTheColumns(ref FabricCutterCBDetailsModel Finalized)
        {
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "CB Number");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "item");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Fabric");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Colour");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Drop");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Tube");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Date-Time");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Width");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "CutWidth");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "B_RColour");
            Finalized.ColumnNames = AddColumnIfNotExists(Finalized.ColumnNames, "Line No");

            foreach (var header in SelectedColumnsNames)
            {
                var Headertext = header.Replace(".", "");

                if (ColumnMapper.ContainsKey(Headertext)) Headertext = ColumnMapper[Headertext];

                if (!Finalized.ColumnNames.Contains(Headertext))
                    Finalized.ColumnNames.Add(Headertext);
            }
        }
    }
}
