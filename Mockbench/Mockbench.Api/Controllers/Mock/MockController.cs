using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;

namespace Mockbench.Server.Controllers.MockControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MockController : ControllerBase
    {
        private readonly ILogger<MockController> _logger;
        private readonly IEndpointRepository _endpointRepository;
        private readonly IHttpService _httpServices;
         
        public MockController(ILogger<MockController> logger, IEndpointRepository endpointRepository, IHttpService httpServices)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _endpointRepository = endpointRepository ?? throw new ArgumentNullException(nameof(endpointRepository));
            _httpServices = httpServices ?? throw new ArgumentNullException(nameof(httpServices));
        }

        //------------------------------------------------------------------
        //  ONE route handles every verb + every 1–3-letter combination
        //------------------------------------------------------------------
        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
        [Route("{code:regex(^[tgm]{1,3}$)}/{**rest}")]
        public async Task<IActionResult> ProxyAsync(string code, string rest)
        {
            _logger.LogTrace("Call to mock  microservice");
            if (!TryParse(code, rest,
                      out var tenantPath, out var environmentPath,
                      out var microservicePath, out var endpointUrl, out var error))
            {
                return BadRequest(error);
            }

            var matchingEndpoint = await _endpointRepository.GetAllMatchingEndpointsAsync(tenantPath, environmentPath, microservicePath, endpointUrl);

            var (isValid, result) = ValidateMatchingResults(matchingEndpoint);

            if (!isValid && result != null)
                return result;

            var restType = HttpContext.Request.Method switch
            {
                "GET" => RestType.GET,
                "POST" => RestType.POST,
                "PUT" => RestType.PUT,
                "PATCH" => RestType.PATCH,
                "DELETE" => RestType.DELETE,
                _ => throw new NotSupportedException()
            };

            var response = await _httpServices.ProcessRequestAsync(matchingEndpoint, restType, HttpContext, endpointUrl);

            HttpContext.Response.Headers.Remove(HttpConstants.CustomBasePathHeaderKey);
            
            return response ?? NotFound();
        }

        //------------------------------------------------------------------
        //  Helpers
        //------------------------------------------------------------------
        private static bool TryParse(
            string code,
            string rest,
            out string? tenant,
            out string? group,
            out string? micro,
            out string endpoint,
            out string? error)
        {
            tenant = group = micro = null;
            endpoint = string.Empty;
            error = null;

            var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < code.Length)
            {
                error = $"Expected {code.Length} path segment(s) " +
                        $"after '{code}', got {parts.Length}.";
                return false;
            }

            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case 't': tenant = parts[i]; break;
                    case 'g': group = parts[i]; break;
                    case 'm': micro = parts[i]; break;
                }
            }

            if (parts.Length > code.Length)
                endpoint = string.Join('/', parts.Skip(code.Length));

            return true;
        }

        /// <summary>
        /// Check Microservice is valid and enabled
        /// </summary>
        /// <param name="matchingRequestMicroserviceDetails"></param>
        /// <returns>return false and bad request if validation fails</returns>
        private (bool isValid, IActionResult? result) ValidateMatchingResults(MatchingEndpoints matchingEndpoint)
        {
            if (matchingEndpoint.Endpoints?.Count == 0)
            {
                return (false, NotFound());
            }

            if (!matchingEndpoint.Environment?.Enabled ?? false)
                return (false, BadRequest(ErrorMessageConstants.EnvironmentDisabled));

            if (!matchingEndpoint.Microservice?.Enabled ?? false)
                return (false, BadRequest(ErrorMessageConstants.MicroserviceDisabled));

            return (true, null);
        }
    }
}