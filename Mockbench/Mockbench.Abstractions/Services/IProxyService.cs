using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;

namespace Mockbench.Abstractions.Services
{
    public interface IProxyService
    {
        Task<IActionResult> ProxyRequestToMicroserviceAsync(MatchingEndpoints matchingEndpoints, RestType restType, HttpContext context, string fullPath);
    }
}