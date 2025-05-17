using Mockbench.Shared.Models.Environment;
using Riok.Mapperly.Abstractions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    public class Environment
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Path { get; set; }


        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [MaxLength(500)]
        public string? DefaultHealthCheckUrl { get; set; }

        public DateTime? SimulateTime { get; set; }

        public List<EnvironmentVariable>? Variables { get; set; }

        public List<Endpoint>? Endpoints { get; set; }
    }
}
