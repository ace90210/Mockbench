using Microsoft.AspNetCore.Http;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Abstractions.Services
{
    public interface IMockService
    {
        Task CreateMockResponseIfNotExistAsync(MatchingEndpoints matchingEndpoints, HttpContext context, RestType restType, string endpointPath, string requestBody, HttpResponseMessage response, TimeSpan latency);

        EndpointDto? FindExactEndpointAsync(MatchingEndpoints matchingEndpoints, HttpContext context, RestType restType, string endpointUrl, string requestBody);

        Task<EndpointDto> GetMatchingEndpointDtoAsync(MatchingEndpoints matchingEndpoints, RestType restType, HttpContext context, string fullPath);

        Task<MockResponseDto> GetMockResponseAsync(MatchingEndpoints matchingEndpoints, RestType restType, HttpContext context, string endpointPath);
    }
}