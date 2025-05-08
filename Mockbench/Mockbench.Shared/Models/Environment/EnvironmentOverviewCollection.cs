using System.Collections.Generic;

namespace Mockbench.Shared.Models.Environment
{
    public class EnvironmentOverviewCollection
    {
        public int TenantId { get; set; }

        public string TenantName { get; set; }

        public List<BasicEnvironmentDto> Environments { get; set; }
    }
}
