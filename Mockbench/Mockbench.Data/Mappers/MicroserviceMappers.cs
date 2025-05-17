using Mockbench.Data.Models;
using Mockbench.Shared.Models.Microservice;
using Riok.Mapperly.Abstractions;

namespace Mockbench.Data.Mappers;

[Mapper]
public partial class MicroserviceMapper
{
    public partial FullMicroserviceDto? ToDto(Microservice? microservice);

    public partial Microservice? ToEntity(FullMicroserviceDto? microservice);

    public partial List<FullMicroserviceDto>? ToDtos(List<Microservice>? microservices);

    public partial List<Microservice>? ToEntities(List<FullMicroserviceDto>? microservices);
}

[Mapper(UseDeepCloning = true)]
public partial class MicroserviceClonerMapper
{
    public partial Microservice? Clone(Microservice? microservice);

    public partial FullMicroserviceDto? Clone(FullMicroserviceDto? microservice);
}
