using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Models
{
    public class Settings
    {
        public int Id { get; set; }       

        public string PreferredTheme { get; set; }

        public UIMode UIMode { get; set; }
    }
}
