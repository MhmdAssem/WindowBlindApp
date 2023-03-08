using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class ModuleSettings
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public bool OverrideForTable { get; set; }
        public bool OverrideForUser { get; set; }
    }
}
