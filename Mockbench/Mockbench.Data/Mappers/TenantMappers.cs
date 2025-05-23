using Mockbench.Data.Models;
using Mockbench.Shared.Models.Tenant;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Mappers;

[Mapper]
public partial class TenantMapper
{
    public partial TenantBaseDto? ToDto(Tenant? tenant);

    public partial Tenant? ToEntity(TenantBaseDto? tenant);

    public partial List<TenantBaseDto>? ToDtos(List<Tenant>? tenants);

    public partial List<Tenant>? ToEntities(List<TenantBaseDto>? tenants);
}

[Mapper(UseDeepCloning = true)]
public partial class TenantClonerMapper
{
    public partial Tenant? Clone(Tenant? tenant);

    public partial TenantBaseDto? Clone(TenantBaseDto? tenant);
}
