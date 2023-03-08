using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class ApplicationTableWithOutputPath
    {
        public string Id { get; set; }
        public string TableName { get; set; }
        public string OutputPath { get; set; }
    }
}
