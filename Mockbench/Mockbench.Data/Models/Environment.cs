using Mockbench.Shared.Models.Environment;
using Riok.Mapperly.Abstractions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    [Mapper]
    public partial class EnvironmentMapper
    {
        public partial EnvironmentDto ToEnvironmentDto(Environment tenant);

        public partial Environment ToEnvironmentEntity(EnvironmentDto tenant);

        public partial List<EnvironmentDto> ToEnvironmentDtos(List<Environment> tenants);

        public partial List<Environment> ToEnvironmentEntities(List<EnvironmentDto> tenants);
    }

    [Mapper(UseDeepCloning = true)]
    public partial class EnvironmentClonerMapper
    {
        public partial Environment Clone(Environment tenant);

        public partial EnvironmentDto Clone(EnvironmentDto tenant);
    }

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
