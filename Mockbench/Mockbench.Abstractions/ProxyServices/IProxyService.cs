using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Abstractions.ProxyServices
{
    public interface IProxyService
    {
        Task<IActionResult> ProxyRequestToMicroserviceAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, RestType restType, HttpContext context, string endpointPath);
    }
}