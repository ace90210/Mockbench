using System.ComponentModel.DataAnnotations;
using Mockbench.Shared.Models.Tenant;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Models
{
    [Mapper]
    public partial class TenantMapper
    {
        public partial FullTenantDto ToTenantDto(Tenant tenant);

        public partial Tenant ToTenantEntity(FullTenantDto tenant);
    }

    [Mapper(UseDeepCloning = true)]
    public partial class TenantClonerMapper
    {
        public partial Tenant Clone(Tenant tenant);

        public partial FullTenantDto Clone(FullTenantDto tenant);
    }

    public class Tenant
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

        public DateTime? SimulateTime { get; set; }
    }
}
