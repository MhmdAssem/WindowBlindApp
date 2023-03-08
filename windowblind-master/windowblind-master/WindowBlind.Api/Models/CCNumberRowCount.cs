using System.Collections.Generic;

namespace WindowBlind.Api.Models
{
    public class CCNumberRowCount
    {
        public int Count { get; set; }
        public List<FabricCutterCBDetailsModelTableRow> rows { get; set; }
    }
}
