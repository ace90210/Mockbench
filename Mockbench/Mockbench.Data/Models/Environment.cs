using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int TenantID { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [MaxLength(500)]
        public string DefaultHealthCheckUrl { get; set; }

        [JsonIgnore]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Tenant Tenant { get; set; }

        public DateTime? SimulateTime { get; set; }

        public List<Microservice> Microservices { get; set; }
    }
}
