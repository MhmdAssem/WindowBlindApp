using AspNetCore.Reporting;
using AspNetCore.Reporting.ReportExecutionService;
using Microsoft.AspNetCore.Hosting;
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
            FinalData = new FabricCutterCBDetailsModel();
        }
        private IRepository _repository;
        private IWebHostEnvironment _env;
        private FabricCutterCBDetailsModel FinalData;
        Dictionary<string, int> ColumnsIndex;
        private Dictionary<string, int> RowColor;
        string ctbsodumpPath, SheetNamePath, FBRPath, DeductionPath, LathePath, FabricPath, DropPath;

        int TotalQty;
        List<string> SelectedColumnsPath;
        int generalBlindNumber;
        Dictionary<string, string> FabricRollwidth;
        Dictionary<string, int> ControlTypevalues, FixingValues;
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
            ColumnMapper.Add("Customer Name 2", "Supplier");
            ColumnMapper.Add("Width", "Measured Width");
            ColumnMapper.Add("Drop", "Measured Drop");
            ColumnMapper.Add("Fabric", "Fabric Type");
            ColumnMapper.Add("Colour", "Fabric Colour");
            ColumnMapper.Add("Pull Type / Control Type /Draw Type", "Control Type");
            ColumnsIndex = new Dictionary<string, int>();
            RowColor = new Dictionary<string, int>();
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
            var DeductionSetting = await _repository.Settings.FindAsync(e => e.settingName == "Deduction");
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

            var SelectedColumnsSetting = await _repository.Settings.FindAsync(e => e.settingName == "SelectedColumnsNames" && e.applicationSetting == "EzStop");
            SelectedColumnsPath = SelectedColumnsSetting.FirstOrDefault().settingPath.Split("@@@").ToList();

            if (SelectedColumnsPath.Count == 1 && SelectedColumnsPath[0].Trim() == "") SelectedColumnsPath = new List<string>();
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

        private FabricCutterCBDetailsModel ReadData()
        {
            FileInfo file = new FileInfo(ctbsodumpPath);
            if (!file.Exists) return null;
            List<string> names = new List<string>();
            //List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
            generalBlindNumber = 1;

            FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();

            FabricRollwidth = new Dictionary<string, string>();
            ControlTypevalues = new Dictionary<string, int>();
            FixingValues = new Dictionary<string, int>();
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
                    var Headertext = worksheet.Cells[1, i].Text.Trim();
                    ColumnsIndex[worksheet.Cells[1, i].Text.Trim()] = i;

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
                    bool takeRowCqty = true;
                    for (int j = start.Column; j <= end.Column; j++)
                    {
                        var Headertext = worksheet.Cells[1, j].Text.Trim();
                        Headertext = Headertext.Replace(".", "");
                        if (String.IsNullOrEmpty(Headertext)) continue;

                        /// special check 
                        if (worksheet.Cells[i, ColumnsIndex["Department"]].Text.Trim() == "") { takeRowCqty = false; RowQty = 0; break; }
                        var cell = worksheet.Cells[i, j].Text.Trim();

                        if (Headertext.Contains("Qty") && cell != "" && takeRowCqty)
                        {
                            RowQty = int.Parse(cell);
                            TotalQty += int.Parse(cell);
                        }
                        if (!Data.ColumnNames.Contains(Headertext) && SelectedColumnsPath.Contains(worksheet.Cells[1, j].Text.Trim()))
                            Data.ColumnNames.Add(Headertext);

                        if (Headertext == ("Colour"))
                        {
                            /// Adding Counting Color Logic 
                            var CurrentRowColor = (worksheet.Cells[i, j].Text.Trim());
                            if (CurrentRowColor != "")
                            {
                                if (!RowColor.ContainsKey(CurrentRowColor)) RowColor[CurrentRowColor] = 0;
                                RowColor[CurrentRowColor]++;
                            }
                        }

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


        private void EzStopProcessing(ref FabricCutterCBDetailsModel Data)
        {
            var f = 0;
            var a = 0;

            var rowCntr = -1;
            foreach (var item in Data.Rows)
            {
                rowCntr++;

                item.Row["RowColorCount"] = "0";
                var CurrentRowColor = item.Row["Fabric Colour"].Trim();
                if (CurrentRowColor != "")
                    item.Row["RowColorCount"] = RowColor[CurrentRowColor].ToString();

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
                //  If New String() {"FIN 31", "FIN 40", "FIN 41"}.Contains(DR("Finish").ToString.Trim)
                if (new List<string> { "FIN 31", "FIN 40", "FIN 41" }.Contains(item.Row["Finish"]))
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
                item.Row["SomeOfTotal"] = a.ToString() + " of " + TotalQty.ToString();
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
                item.Row["FromHoldingStation"] = "NO";

            }

            Data.ColumnNames = SelectedColumnsPath;
            //Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "CB Number");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "item");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Qty");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Width");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "CutWidth");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Tube");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Spring");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Finish");
            Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "Colour");
        }


        [HttpGet("getCBNumberDetails")]
        public async Task<IActionResult> getCBNumberDetails(string CBNumberOrLineNumber)
        {
            try
            {
                CBSearch = false;
                CB = CBNumberOrLineNumber;
                LineNumber = CBNumberOrLineNumber;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ColumnsIndex = new Dictionary<string, int>();

                Init();
                await ReadConfig();


                bool checks = CheckPaths();
                if (!checks) return new JsonResult(false);


                var Data = ReadData();


                EzStopProcessing(ref Data);

                Data.Rows = Data.Rows.Where(e => e.Row["Line No"] == CBNumberOrLineNumber).ToList();


                EzStopModel model = new EzStopModel
                {
                    id = Guid.NewGuid().ToString(),
                    data = Data
                };
                _repository.EzStopData.InsertOne(model);


                FinalData.ColumnNames = SelectedColumnsPath;
                //Data.ColumnNames = LogCut.AddColumnIfNotExists(Data.ColumnNames, "CB Number");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "item");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Qty");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Width");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "CutWidth");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Tube");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Spring");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Finish");
                FinalData.ColumnNames = LogCut.AddColumnIfNotExists(FinalData.ColumnNames, "Colour");


                return new JsonResult(FinalData);

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
                var XMLSetting = await _repository.Settings.FindAsync(e => e.settingName == "XML Folder");
                var XMLPath = XMLSetting.FirstOrDefault().settingPath;
                DirectoryInfo Dir = new DirectoryInfo(XMLPath);
                if (!Dir.Exists) new JsonResult(false);

                foreach (var file in Dir.GetFiles())
                {
                    XmlSerializer reader =
                  new XmlSerializer(typeof(OrderObject));
                    System.IO.StreamReader file32 = new StreamReader(file.FullName);
                    var result = (OrderObject)reader.Deserialize(file32);
                    file32.Close();
                    file.Delete();
                    await getCBNumberDetails(result.OrderNumber);

                }
                /// Getting Data from the DB

                var EzStopData = _repository.EzStopData.Find(e => e.data != null).ToList();

                for (int i = 0; i < EzStopData.Count; i++)
                {
                    FinalData.ColumnNames = (EzStopData[i].data.ColumnNames.Count > FinalData.ColumnNames.Count) ? EzStopData[i].data.ColumnNames : FinalData.ColumnNames;
                    if (EzStopData[i].data.Rows.Count > 0) EzStopData[i].data.Rows[0].UniqueId = EzStopData[i].id;
                    FinalData.Rows.AddRange(EzStopData[i].data.Rows);
                }

                return new JsonResult(FinalData);

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
                var EzStopOutputSetting = await _repository.Tables.FindAsync(e => e.TableName == model.tableName);
                var EzStopFilePath = EzStopOutputSetting.FirstOrDefault().OutputPath;
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
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Done"), new UpdateOptions { IsUpsert = false });

                    await _repository.EzStopData.DeleteOneAsync(e => e.id == item.UniqueId);
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
                for (int i = 0; i < strParameterArray.Count; i++)
                {
                    while (strParameterArray[i].IndexOf("  ") != -1)
                        strParameterArray[i] = strParameterArray[i].Replace("  ", " ");
                    if (String.IsNullOrEmpty(strParameterArray[i]))
                        strParameterArray[i] = " ";
                }

                string mimtype = "";
                int extension = 1;
                var path = Path.Combine("E:\\Webapp_input files", "Printer Driver", StrReportPath);
                //var path = Path.Combine("F:\\FreeLance\\BlindsWebapp\\windowblind-master\\windowblind-master\\PrinterProject", StrReportPath);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("us-ascii");
                var parametersList = new Dictionary<string, string>();

                parametersList.Add("someoftotal", strParameterArray[11] + " of " + strParameterArray[12].ToString());
                parametersList.Add("char", strParameterArray[9].ToString());
                parametersList.Add("lathe", strParameterArray[8].ToString());
                parametersList.Add("controltype", strParameterArray[6].ToString());
                parametersList.Add("color", strParameterArray[7].ToString());
                parametersList.Add("fabric", strParameterArray[5].ToString());
                parametersList.Add("department", strParameterArray[4].ToString());
                parametersList.Add("customer", strParameterArray[3].ToString());
                parametersList.Add("drop", strParameterArray[2].ToString());
                parametersList.Add("width", strParameterArray[1].ToString());
                parametersList.Add("cbNumber", strParameterArray[0].ToString());


                LocalReport report = new LocalReport(path);


                byte[] result = report.Execute(RenderType.Image, extension, parametersList, mimtype).MainStream;

                var outputPath = Path.Combine("E:\\Webapp_input files", "Printer Driver", "EzStopPrintFiles", Guid.NewGuid().ToString() + ".jpg");
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
                        m.Y = 0;// (int)((((System.Drawing.Printing.PrintDocument)(sndr)).DefaultPageSettings.PaperSize.Height - m.Height) / 2);
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
        public async Task<IActionResult> GetHeldObjects([FromHeader] string tableName)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                CBSearch = true;
                ColumnsIndex = new Dictionary<string, int>();

                Init();
                await ReadConfig();

                var check = CheckPaths();




                FabricCutterCBDetailsModel Data = new FabricCutterCBDetailsModel();


                var HeldObjects = await _repository.Rejected.FindAsync(rej => rej.ForwardedToStation == "EzStop" && rej.TableName == tableName).Result.ToListAsync();

                foreach (var item in HeldObjects)
                {
                    CB = item.Row.Row["CB Number"];
                    var HeldData = ReadData();
                    EzStopProcessing(ref HeldData);


                    Data.Rows.Add(HeldData.Rows.Where(e => e.Row["Line No"] == item.Row.Row["Line No"]).FirstOrDefault());
                    Data.Rows[Data.Rows.Count - 1].Row["FromHoldingStation"] = "YES";
                    Data.Rows[Data.Rows.Count - 1].UniqueId = item.Row.UniqueId;

                    Data.ColumnNames = HeldData.ColumnNames;
                }



                return new JsonResult(Data);

            }
            catch (Exception e)
            {

                return new JsonResult(false);
            }
        }

        [HttpPost("ClearOrdersFromEzStop")]
        public async Task<IActionResult> ClearOrdersFromEzStop([FromBody] CreateFileAndLabelModel model)
        {
            try
            {
                foreach (var item in model.data.Rows)
                {
                    if (item.Row["FromHoldingStation"] == "YES")
                        await _repository.Rejected.UpdateOneAsync(rej => rej.Id == item.UniqueId,
                                            Builders<RejectionModel>.Update.Set(p => p.ForwardedToStation, "Deleted By: " + model.userName), new UpdateOptions { IsUpsert = false });

                    await _repository.EzStopData.DeleteOneAsync(e => e.id == item.UniqueId);
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
