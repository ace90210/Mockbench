using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    public class EnvironmentVariable
    {
        [Key]
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int EnvironmentId { get; set; }

        public Environment Environment { get; set; }
    }
}
