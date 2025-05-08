using Mockbench.Shared.Models.Tenant;
using System.Collections.Generic;

namespace Mockbench.Shared.Models.Microservice
{
    public class MockTestingDto
    {
        public List<BasicTenantDto> Tenants { get; set; }
    }
}
