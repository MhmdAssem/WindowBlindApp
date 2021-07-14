using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace POS_Service.Extra
{
    static class Reader
    {
        // the site reader that get the site id form the text TableReader.txt"on the c partition
        public static List<string> TableReader()
        {
            var data = File.ReadAllLines(@"C:\ComPortService\Table.txt");
            return data.ToList();
        }
    }
}
