using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    public class TenantVariable
    {
        [Key]
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int TenantId { get; set; }

        public Tenant Tenant { get; set; }
    }
}
