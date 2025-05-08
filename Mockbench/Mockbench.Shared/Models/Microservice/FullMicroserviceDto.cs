using System.Collections.Generic;
using Mockbench.Shared.Models.Endpoint;

namespace Mockbench.Shared.Models.Microservice
{
    public class FullMicroserviceDto : MicroserviceResultDto
    {
        public List<EndpointDto> Endpoints { get; set; }
    }
}
