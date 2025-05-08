using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Abstractions.ProxyServices
{
    public interface IProxyService
    {
        Task<IActionResult> ProxyRequestToMicroserviceAsync(MatchingEndpointMicroserviceDetailsDto microservice, RestType restType, HttpContext context, string endpointPath);
    }
}