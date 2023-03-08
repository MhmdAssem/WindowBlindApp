using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class Station
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public FabricCutterSettings Settings { get; set; }

        public List<Table> Tables { get; set; }

        public class FabricCutterSettings
        {
            public Dictionary<string, string> Lathe { get; set; }
            public Dictionary<string, int> RollWidth { get; set; }
            public Dictionary<string, int> WidthDeductions { get; set; }
            public string DumpFilePath { get; set; }
            public string DumpSheetName { get; set; }
            
        }
    }
}