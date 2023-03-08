using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class DropTableModel
    {
        public int From{ get; set; }
        public int To{ get; set; }
        public string DropGroup{ get; set; }
        public string DropColour{ get; set; }

        public DropTableModel()
        {
            From = 0;
            To = 0;
            DropGroup = "";
            DropColour = "";
        }
    }
}
