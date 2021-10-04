using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class OrdersApprovalModel
    {
        public string forwardStation { get; set; }
        public List<RejectionModel> data{ get; set; }
}
}
