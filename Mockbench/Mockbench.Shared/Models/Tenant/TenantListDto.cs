using System.Collections.Generic;

namespace Mockbench.Shared.Models.Tenant
{
    public class TenantListDto
    {
        public int TotalTenants { get; set; }

        public List<TenantBaseDto> Tenants { get; set; } = new List<TenantBaseDto>();
    }
}
