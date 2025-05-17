using Mockbench.Shared.Models.Environment;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Mappers;

[Mapper]
public partial class EnvironmentMapper
{
    public partial EnvironmentDto? ToDto(Models.Environment? environment);

    public partial Models.Environment? ToEntity(EnvironmentDto? environment);

    public partial List<EnvironmentDto>? ToDtos(List<Models.Environment>? environments);

    public partial List<Models.Environment>? ToEntities(List<EnvironmentDto>? environments);
}

[Mapper(UseDeepCloning = true)]
public partial class EnvironmentClonerMapper
{
    public partial Models.Environment? Clone(Models.Environment? environment);

    public partial EnvironmentDto? Clone(EnvironmentDto? environment);
}
