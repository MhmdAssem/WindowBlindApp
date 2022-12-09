using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrstringerProject
{
    public class LogCut2
    {
        public string width { get; set; }
        public string drop { get; set; }
        public string customer { get; set; }
        public string department { get; set; }
        public string type { get; set; }
        public string fabric { get; set; }
        public string color { get; set; }
        public string controltype { get; set; }
        public string lathe { get; set; }
        public string c { get; set; }
        public string cbNumber { get; set; }
        public string someoftotal { get; set; }
    }

    public class LogCut1
    {
        public string width { get; set; }
        public string drop { get; set; }
        public string customer { get; set; }
        public string department { get; set; }
        public string type { get; set; }
        public string fabric { get; set; }
        public string color { get; set; }
        public string controltype { get; set; }
        public string lathe { get; set; }
        public string c { get; set; }
        public string cbNumber { get; set; }
        public string someoftotal { get; set; }
        public string lineNumber { get; set; }
        public string cutwidth { get; set; }
        public string cntrside { get; set; }

    }

    public class FabricCut
    {
        public string width { get; set; }
        public string drop { get; set; }
        public string customer { get; set; }
        public string department { get; set; }
        public string type { get; set; }
        public string fabric { get; set; }
        public string color { get; set; }
        public string controltype { get; set; }
        public string lathe { get; set; }
        public string c { get; set; }
        public string cbNumber { get; set; }
        public string someoftotal { get; set; }
        public string lineNumber { get; set; }
        public string cutwidth { get; set; }
        public string cntrside { get; set; }
    }

    public class EzStop
    {
        public string width { get; set; }
        public string drop { get; set; }
        public string customer { get; set; }
        public string department { get; set; }
        public string fabric { get; set; }
        public string color { get; set; }
        public string controltype { get; set; }
        public string lathe { get; set; }
        public string c { get; set; }
        public string cbNumber { get; set; }
        public string someoftotal { get; set; }
    }

    public class PinkLabel
    {
        public string PO { get; set; }
        public string CCNumber { get; set; }
        public string Cust { get; set; }
        public string CustRef { get; set; }
        public string Supplier { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Width { get; set; }
        public string Drop { get; set; }
        public string LineNumber { get; set; }
        public string SomeOfTotal { get; set; }
        public string Customer { get; set; }
        public string Carrier { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string PostCode { get; set; }
        public string Status { get; set; }
    }
    public class BigLabel
    {
        public string CBNumber { get; set; }
        public string Carrier { get; set; }
        public string Customer { get; set; }
        public string PO { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Width { get; set; }
        public string Drop { get; set; }
        public string SomeOfTotal { get; set; }
        public string FittingAddress { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
    }
}
