using Mockbench.Data.Models;
using Mockbench.Shared.Models.Tenant;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Mappers;

[Mapper]
public partial class TenantMapper
{
    public partial TenantBase? ToDto(Tenant? tenant);

    public partial Tenant? ToEntity(TenantBase? tenant);

    public partial List<TenantBase>? ToDtos(List<Tenant>? tenants);

    public partial List<Tenant>? ToEntities(List<TenantBase>? tenants);
}

[Mapper(UseDeepCloning = true)]
public partial class TenantClonerMapper
{
    public partial Tenant? Clone(Tenant? tenant);

    public partial TenantBase? Clone(TenantBase? tenant);
}
