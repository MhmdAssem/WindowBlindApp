using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class TablePrinterModel
    {
        public string TableName { get; set; }
        public string PrinterName { get; set; }
        public string ApplicationName { get; set; }

        public TablePrinterModel() { }
    }
}
