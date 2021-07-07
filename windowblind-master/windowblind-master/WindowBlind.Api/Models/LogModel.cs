using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class LogModel
    {
        public string Id{ get; set; }
        public string ProcessType { get; set; }
        public string LineNumber { get; set; }
        public string CBNumber { get; set; }
        public string Item { get; set; }
        public string Message { get; set; }
        public string TableName { get; set; }
        public string UserName { get; set; }

        public string dateTime { get; set; }
        public FabricCutterCBDetailsModelTableRow row { get; set; }
        public LogModel()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
