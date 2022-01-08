using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class RejectionModel
    {
        public string Id { get; set; }
        public string StationName { get; set; }
        public string DateTime { get; set; }
        public FabricCutterCBDetailsModelTableRow Row { get; set; }
        public string UserName { get; set; }
        public string TableName { get; set; }

        public List<string> RejectionReasons { get; set; }

        public string ForwardedToStation { get; set; }

    }
}
