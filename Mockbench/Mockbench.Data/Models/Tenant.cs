using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    public class Tenant
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Path { get; set; }

        public DateTime? SimulateTime { get; set; }

        public List<TenantVariable>? Variables { get; set; } = new List<TenantVariable>();

        public List<Endpoint>? Endpoints { get; set; }
    }
}
