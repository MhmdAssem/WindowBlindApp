using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowBlind.Api.Models;

namespace WindowBlind.Api
{
    public interface IRepository
    {
        IMongoCollection<Order> Orders { get; }
        IMongoCollection<User> Users { get; }
        IMongoCollection<Station> Stations { get; }
        IMongoCollection<FileSetting> Settings { get; }
        IMongoCollection<LogModel> Logs { get; }
        IMongoCollection<LogModel> PackingStation { get; }
        IMongoCollection<LogModel> HoistStation { get; }
        IMongoCollection<LogModel> AssemblyStation { get; }

        Task ImportOrders(Station station);
        void Seed();
    }
    public class Repository : IRepository
    {
        public IMongoCollection<Order> Orders => _mongoDb.GetCollection<Order>("orders");
        public IMongoCollection<User> Users => _mongoDb.GetCollection<User>("users");
        public IMongoCollection<Station> Stations => _mongoDb.GetCollection<Station>("stations");
        public IMongoCollection<FileSetting> Settings => _mongoDb.GetCollection<FileSetting>("settings");
        public IMongoCollection<LogModel> Logs => _mongoDb.GetCollection<LogModel>("logs");
        public IMongoCollection<LogModel> AssemblyStation => _mongoDb.GetCollection<LogModel>("AssemblyStation");
        public IMongoCollection<LogModel> HoistStation => _mongoDb.GetCollection<LogModel>("HoistStation");
        public IMongoCollection<LogModel> PackingStation => _mongoDb.GetCollection<LogModel>("PackingStation");


        private readonly IMongoDatabase _mongoDb;

        public Repository(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _mongoDb = client.GetDatabase(settings.DatabaseName);
            Console.WriteLine("Connect to MongoDB");
            Seed();
        }

        public void Seed()
        {
            var user = _mongoDb.GetCollection<User>("users").EstimatedDocumentCount();
            var FileSettings = _mongoDb.GetCollection<FileSetting>("settings").EstimatedDocumentCount();
            var Logs = _mongoDb.GetCollection<FileSetting>("logs").EstimatedDocumentCount();
            var PackingStation = _mongoDb.GetCollection<FileSetting>("PackingStation").EstimatedDocumentCount();
            var HoistStation = _mongoDb.GetCollection<FileSetting>("HoistStation").EstimatedDocumentCount();
            var AssemblyStation = _mongoDb.GetCollection<FileSetting>("AssemblyStation").EstimatedDocumentCount();
            if (user == 0)
            {
                SeedUsers();
            }
            if (FileSettings == 0)
            {
                SeedSettings();
            }
            if (Logs == 0)
            {
                seedLogs();
            }
            if (PackingStation == 0)
            {
                seedPackingStation();
            }
            if (HoistStation == 0)
            {
                seedHoistStation();
            }
            if (AssemblyStation == 0)
            {
                seedAssemblyStation();
            }

        }

        private void SeedUsers()
        {
            var user = new User();
            user.Id = Guid.NewGuid().ToString();
            user.Name = "Admin";
            user.Role = "Admin";
            user.Password = Guid.NewGuid().ToString();
            _mongoDb.GetCollection<User>("users").InsertOne(user);
        }

        private void SeedSettings()
        {
            var setting = new List<FileSetting>
            {
                new FileSetting{Id = Guid.NewGuid().ToString(), settingName = "ctbsodump",settingPath = ""},
                new FileSetting{Id = Guid.NewGuid().ToString(), settingName = "SheetName",settingPath = ""},
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "Deduction",settingPath = "" },
                new FileSetting{ Id = Guid.NewGuid().ToString(),settingPath = "",settingName = "DeductionTable"},
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "DropTable" , settingPath = "" },
                new FileSetting{ Id = Guid.NewGuid().ToString(),settingPath = "",settingName = "Fabric Rollwidth"},
                new FileSetting{ Id = Guid.NewGuid().ToString(),settingName = "FabricTable" , settingPath = ""},
                new FileSetting{ Id = Guid.NewGuid().ToString(),settingPath = "",settingName = "InputTable"},
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "PVCLathe Fabric",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "Fabric Cutter Output",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "LogCut Output",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "EzStop Output",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "XML File",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "SelectedColumnsNames",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "FabricCutterTable",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "LogCutterTable",settingPath = "" },
                new FileSetting{Id = Guid.NewGuid().ToString(),settingName = "EzStopTable",settingPath = "" }
            };
            _mongoDb.GetCollection<FileSetting>("settings").InsertMany(setting);
        }

        private void seedLogs()
        {
            LogModel log = new LogModel();
            log.UserName = "Admin";
            log.TableName = "";
            log.Message = "The Admin Just LoggedIn to the system";
            log.dateTime = DateTime.Now.ToString();
            _mongoDb.GetCollection<LogModel>("logs").InsertOne(log);

        }

        public async Task ImportOrders(Station station)
        {
            string Con_Str = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Path.GetFullPath(station.Settings.DumpFilePath)};Mode=ReadWrite;Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
            var lathe = station.Settings.Lathe != null ?
                station.Settings.Lathe.SelectMany(d => d.Value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(v => $"{d.Key}$$${v.Trim()}")).ToList() :
                new List<string>();
            using (var connection = new OleDbConnection(Con_Str))
            {
                connection.Open();
                var query = $"select [Department],[W/Order NO],[Customer Name 1],[Qty],[Width],[Drop],[Bind# Type/# Panels/Rope/Operation],[Fabric],[Description],[Colour],[Pull Colour/Bottom Weight/Wand Len],[Colour],[Track Col/Roll Type/Batten Col],[Pull Colour/Bottom Weight/Wand Len],[Line No#],[Cntrl Side] from [{station.Settings.DumpSheetName}$] WHERE NOT TRIM([Department]) = ''";
                OleDbCommand cm = new OleDbCommand(query, connection);
                cm.CommandText = query;
                var rdr = await cm.ExecuteReaderAsync();
                DataTable dt = new DataTable();

                dt.Load(rdr);

                var orders = dt.Rows.OfType<DataRow>().GroupBy(r => new
                {
                    CBNumber = r["W/Order NO"].ToString().TrimEnd(),
                    Department = r["Department"].ToString().TrimEnd(),
                    Customer = r["Customer Name 1"].ToString().TrimEnd()
                }
                );
                var widthRegex = new Regex(@"\s(\d+)MM$");

                foreach (var orderGroup in orders)
                {
                    int pieceCounter = 1;
                    int positionCounter = 1;
                    var order = new Order
                    {
                        CBNumber = orderGroup.Key.CBNumber,
                        Department = orderGroup.Key.Department,
                        Customer = orderGroup.Key.Customer,
                        Positions = orderGroup.Select(dr =>
                        {
                            var qty = Convert.ToInt32(dr["Qty"].ToString());
                            var range = new OrderPosition.NumberRange
                            {
                                From = pieceCounter,
                                To = pieceCounter + qty - 1
                            };
                            pieceCounter += qty;
                            var controlSide = dr["Cntrl Side"].ToString().TrimEnd();
                            var controlType = dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd();
                            var measuredWidth = Convert.ToInt32(dr["Width"].ToString().TrimEnd());
                            var pullColour = dr["Pull Colour/Bottom Weight/Wand Len"].ToString().TrimEnd();
                            var fabricColour = dr["Colour"].ToString().TrimEnd();
                            var fabric = dr["Fabric"].ToString().TrimEnd();
                            var widthMatch = widthRegex.Match(fabric);
                            var description = dr["Description"].ToString().TrimEnd();
                            return new OrderPosition
                            {
                                Barcode = dr["Line No#"].ToString().TrimEnd(),
                                BlindNumberRange = range,
                                ControlSide = String.IsNullOrEmpty(controlSide) ? "N" : controlSide.Substring(0, 1),
                                ControlType = String.IsNullOrEmpty(controlType) ? "Pin" : controlType,
                                //TODO: Implement width deductions
                                CutWidth = Convert.ToInt32(dr["Width"])
                                - (station.Settings.WidthDeductions?.ContainsKey(controlType) ?? false ? station.Settings.WidthDeductions[controlType] : 30),
                                FabricColour = fabricColour,
                                FabricType = fabric,
                                LineNumber = positionCounter++,
                                MeasuredDrop = dr["Drop"].ToString().TrimEnd(),
                                MeasuredWidth = dr["Description"].ToString().TrimEnd().EndsWith("FIN 36") && controlType == "Motorised" ?
                                    measuredWidth - 5 : measuredWidth,
                                //TODO: lathe types
                                PullColour = String.IsNullOrEmpty(pullColour) ?
                                    (lathe.Contains($"{fabric}$$${fabricColour}") ? "PVC Lathe" : "Lathe")
                                     : pullColour,
                                Quantity = qty,
                                //TODO: roll width configuration
                                RollWidth = widthMatch.Success ?
                                    (int?)(station.Settings.RollWidth?.ContainsKey(widthMatch.Groups[1].Value) ?? false ?
                                        station.Settings.RollWidth[widthMatch.Groups[1].Value]
                                        : Convert.ToInt32(widthMatch.Groups[1].Value))
                                 : null,
                                TrackColour = dr["Track Col/Roll Type/Batten Col"].ToString().TrimEnd(),
                                TrimType = description.Substring(description.Length - 6, 6)
                            };

                        }).ToList()
                    };
                    await _mongoDb.GetCollection<Order>("Orders").InsertOneAsync(order);
                }



            }

        }

        private void seedPackingStation()
        {
            LogModel log = new LogModel();
            log.UserName = "Admin";
            log.TableName = "";
            log.Message = "The Admin Just LoggedIn to the system";
            log.dateTime = DateTime.Now.ToString();
            _mongoDb.GetCollection<LogModel>("PackingStation").InsertOne(log);

        }
        private void seedHoistStation()
        {
            LogModel log = new LogModel();
            log.UserName = "Admin";
            log.TableName = "";
            log.Message = "The Admin Just LoggedIn to the system";
            log.dateTime = DateTime.Now.ToString();
            _mongoDb.GetCollection<LogModel>("HoistStation").InsertOne(log);

        }

        private void seedAssemblyStation()
        {
            LogModel log = new LogModel();
            log.UserName = "Admin";
            log.TableName = "";
            log.Message = "The Admin Just LoggedIn to the system";
            log.dateTime = DateTime.Now.ToString();
            _mongoDb.GetCollection<LogModel>("AssemblyStation").InsertOne(log);

        }


    }
}
