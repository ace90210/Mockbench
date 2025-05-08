using Microsoft.AspNetCore.Http;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Abstractions.MockServices
{
    public interface IMockService
    {
        Task CreateMockResponseIfNotExistAsync(MicroserviceResultDto microservice, HttpContext context, RestType restType, string endpointPath, string requestBody, HttpResponseMessage response, TimeSpan latency);

        Task<EndpointDto> FindMatchingEndpointAsync(int microserviceId, HttpContext context, RestType restType, string endpoint, string requestBody);
        
        Task<EndpointDto> GetMatchingEndpointDtoAsync(MicroserviceResultDto microservice, RestType restType, HttpContext context, string endpointPath);

        Task<MockResponseDto> GetMockResponseAsync(MicroserviceResultDto microservice, RestType restType, HttpContext context, string endpointPath);
    }
}