using Microsoft.AspNetCore.Http;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Response;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Abstractions.MockServices
{
    public interface IMockService
    {
        Task CreateMockResponseIfNotExistAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, HttpContext context, RestType restType, string endpointPath, string requestBody, HttpResponseMessage response, TimeSpan latency);

        Task<EndpointDto> FindMatchingEndpointAsync(string? tenantPath, string? environmentPath, string? microservicePath, HttpContext context, RestType restType, string endpoint, string requestBody);
        
        Task<EndpointDto> GetMatchingEndpointDtoAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, RestType restType, HttpContext context, string endpointPath);

        Task<MockResponseDto> GetMockResponseAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, RestType restType, HttpContext context, string endpointPath);
    }
}