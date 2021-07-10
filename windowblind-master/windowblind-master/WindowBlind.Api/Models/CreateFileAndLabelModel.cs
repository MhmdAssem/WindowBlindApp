using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class CreateFileAndLabelModel
    {
        public string tableName { get; set; }
        public string userName { get; set; }
        public string printer { get; set; }

        public FabricCutterCBDetailsModel data { get; set; }
    }
}
