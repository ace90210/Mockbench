using Mockbench.Data.Models;
using Mockbench.Shared.Models.Microservice;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Mappers;

[Mapper]
public partial class MicroserviceMapper
{
    public partial MicroserviceDto? ToDto(Microservice? microservice);

    public partial Microservice? ToEntity(MicroserviceDto? microservice);

    public partial List<MicroserviceDto>? ToDtos(List<Microservice>? microservices);

    public partial List<Microservice>? ToEntities(List<MicroserviceDto>? microservices);
}

[Mapper(UseDeepCloning = true)]
public partial class MicroserviceClonerMapper
{
    public partial Microservice? Clone(Microservice? microservice);

    public partial MicroserviceDto? Clone(MicroserviceDto? microservice);
}
