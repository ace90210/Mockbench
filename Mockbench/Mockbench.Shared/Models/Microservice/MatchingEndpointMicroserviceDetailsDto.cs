using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Shared.Models.Microservice
{
    public class MatchingEndpointMicroserviceDetailsDto
    {
        public MicroserviceResultDto Microservice { get; set; }

        public BaseTenantDto Tenant { get; set; }

        public BasicEnvironmentDto Environment { get; set; }
    }
}
