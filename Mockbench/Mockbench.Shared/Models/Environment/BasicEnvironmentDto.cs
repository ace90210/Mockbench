using Mockbench.Shared.Models.Microservice;
using System.Collections.Generic;

namespace Mockbench.Shared.Models.Environment
{
    public class BasicEnvironmentDto : BaseEnvironmentDto
    {
        public List<MicroserviceResultDto> Microservices { get; set; }
    }
}
