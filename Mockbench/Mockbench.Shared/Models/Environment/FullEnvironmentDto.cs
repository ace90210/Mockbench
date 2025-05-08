using Mockbench.Shared.Models.Microservice;
using System.Collections.Generic;

namespace Mockbench.Shared.Models.Environment
{
    public class FullEnvironmentDto : BaseEnvironmentDto
    {
        public List<FullMicroserviceDto> Microservices { get; set; }
    }
}
