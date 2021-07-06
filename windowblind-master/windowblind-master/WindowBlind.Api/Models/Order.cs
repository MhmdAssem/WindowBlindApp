using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    [BsonIgnoreExtraElements]
    public class Order
    {
        public string Department { get; set; }
        public string CBNumber { get; set; }
        public string Customer { get; set; }
        public List<OrderPosition> Positions { get; set; }
    }
}
