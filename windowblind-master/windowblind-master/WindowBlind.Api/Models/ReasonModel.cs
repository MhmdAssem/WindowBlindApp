using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class ReasonModel
    {
        public string reason { get; set; }
        public string originalStation { get; set; }
        public string orderid { get; set; }
        public bool addOrRemove  { get; set; }
    }
}
