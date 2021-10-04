using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class AutoUploadModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string UserName { get; set; }
        public string Shift { get; set; }
        public string TableName { get; set; }
        public string CreationDate { get; set; }
        public string Type{ get; set; }
        public FabricCutterCBDetailsModelTableRow row { get; set; }

    }
}
