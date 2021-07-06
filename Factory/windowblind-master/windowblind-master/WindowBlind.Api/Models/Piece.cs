using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class Piece
    {
        public int BlindNumber { get; set; }
        public string Barcode { get; set; }
        public int LineNumber { get; set; }
        public int Quantity { get; set; }
        public int MeasuredWidth { get; set; }
        public string MeasuredDrop { get; set; }
        public string ControlType { get; set; }
        //declared but never used in the old app
        //public string Hem { get; set; }
        public string FabricType { get; set; }
        public string FabricColour { get; set; }
        public string TrimType { get; set; }
        public string ControlSide { get; set; }
        public string TrackColour { get; set; }
        public int RollWidth { get; set; }
        public string PullColour { get; set; }
        public string TableNumber { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public int CutWidth { get; set; }

        public string Department { get; set; }
        public string CBNumber { get; set; }

    }
}
