using System.Collections.Generic;

namespace WindowBlind.Api.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public UserSettings Settings { get; set; }
    }

    public class UserSettings
    {
        public List<GridColumn> FabricCutterColumns { get; set; }
    }
    public class GridColumn
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }
}
