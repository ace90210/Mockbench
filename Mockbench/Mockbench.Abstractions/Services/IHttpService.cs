using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;

namespace Mockbench.Abstractions.Services
{
    public interface IHttpService
    {
        Task<IActionResult> ProcessRequestAsync(MatchingEndpoints matchingEndpoint, RestType restType, HttpContext context, string path);
    }
}