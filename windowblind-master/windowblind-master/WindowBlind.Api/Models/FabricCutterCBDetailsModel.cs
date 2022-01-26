using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class FabricCutterCBDetailsModel
    {
        public List<string> ColumnNames { get; set; }
        public List<FabricCutterCBDetailsModelTableRow> Rows { get; set; }
        public FabricCutterCBDetailsModel()
        {
            ColumnNames = new List<string>();
            Rows = new List<FabricCutterCBDetailsModelTableRow>();
        }

    }

    public class FabricCutterCBDetailsModelTableRow
    {
        public string UniqueId { set; get; }
        public Dictionary<string, string> Row { get; set; }
        public List<int> BlindNumbers { get; set; }
        public string PackingType { get; set; }
        public List<string> rows_AssociatedIds { get; set; }

        public FabricCutterCBDetailsModelTableRow()
        {
            BlindNumbers = new List<int>();
            Row = new Dictionary<string, string>();
            rows_AssociatedIds = new List<string>();

        }

    }



}
