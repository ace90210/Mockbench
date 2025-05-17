using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Shared.Models.Endpoint
{
    public class MatchingEndpoints
    {
        public TenantBase? Tenant { get; set; }

        public string? TenantPath { get; set; }

        public EnvironmentDto? Environment { get; set; }

        public string? EnvironmentPath { get; set; }

        public MicroserviceDto? Microservice { get; set; }

        public string? MicroservicePath { get; set; }

        public List<EndpointDto> Endpoints { get; set; }
    }
}
