using System.Collections.Generic;

namespace Mockbench.Shared.Models.Tenant
{
    public class TenantListDto
    {
        public int TotalTenants { get; set; }

        public List<TenantBase> Tenants { get; set; }
    }
}
