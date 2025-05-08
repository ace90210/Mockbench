using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Microservice;
using Riok.Mapperly.Abstractions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Data.Models
{
    [Mapper]
    public partial class MicroserviceMapper
    {
        public partial FullMicroserviceDto ToMicroserviceDto(Microservice tenant);

        public partial Microservice ToMicroserviceEntity(FullMicroserviceDto tenant);

        public partial List<FullMicroserviceDto> ToMicroserviceDtos(List<Microservice> tenants);

        public partial List<Microservice> ToMicroserviceEntities(List<FullMicroserviceDto> tenants);
    }

    [Mapper(UseDeepCloning = true)]
    public partial class MicroserviceClonerMapper
    {
        public partial Microservice Clone(Microservice tenant);

        public partial FullMicroserviceDto Clone(FullMicroserviceDto tenant);
    }

    public class Microservice
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Path { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int MicroserviceID { get; set; }

        [MaxLength(450)]
        public string TargetUrl { get; set; } = string.Empty;

        public int FakeDelay { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        public ProxyMode ProxyMode { get; set; }

        public DateTime? SimulateTime { get; set; }

        public bool RandomiseMockResult { get; set; }

        public bool PassThroughTenant { get; set; }

        public HeadersMode HeadersMode { get; set; }

        public bool InjectForwardingHeadersOnRequest { get; set; }

        public List<ServiceHeader> Headers { get; set; }

        public List<Endpoint> Endpoints { get; set; }
    }
}
