using Mockbench.Data.Models;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Data.Mappers
{
    public static class MicroserviceMappers
    {
        public static MicroserviceResultDto? ToDto(this Microservice microservice, int environmentId)
        {
            return microservice == null ? null : new MicroserviceResultDto()
            {
                Id = microservice.Id,
                Name = microservice.Name,
                Path = microservice.Path,
                Enabled = microservice.Enabled,
                ProxyMode = microservice.ProxyMode,
                RandomiseMockResult = microservice.RandomiseMockResult,
                FakeDelay = microservice.FakeDelay,
                TargetUrl = microservice.TargetUrl,
                SimulateTime = microservice.SimulateTime,
                PassThroughTenant = microservice.PassThroughTenant,
                HeadersMode = microservice.HeadersMode,
                InjectForwardingHeadersOnRequest = microservice.InjectForwardingHeadersOnRequest,
                Headers = microservice.Headers?.ToDtos()
            };
        }

        public static List<MicroserviceResultDto?>? ToDtos(this List<Microservice> microservice, int environmentId)
        {
            return microservice?.Select(rr => rr.ToDto(environmentId)).ToList();
        }
    }
}
