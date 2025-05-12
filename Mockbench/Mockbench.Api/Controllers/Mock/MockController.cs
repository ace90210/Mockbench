using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Api.Helpers;
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
        [Route("{code:regex(^$|^[[tem]]{{1,3}}$)}/{**rest}")]
        [Route("{**rest}")]
        public async Task<IActionResult> ProxyAsync(string? code, string? rest)
        {
            _logger.LogTrace("Call to mock  microservice");

            // Ensure 'code' is treated as empty string if null for TryParseParamCodes logic
            string currentCode = code ?? string.Empty;
            string currentRest = rest ?? string.Empty;

            if (!HelperExtensions.TryParseParamCodes(currentCode, currentRest,
                                     out var tenantPath, out var environmentPath,
                                     out var microservicePath, out var endpointUrl, out var error))
            {
                return BadRequest(error);
            }

            var matchingEndpoint = await _endpointRepository.GetAllMatchingEndpointsAsync(tenantPath, environmentPath, microservicePath, endpointUrl);

            var (isValid, result) = ValidateMatchingResults(matchingEndpoint);

            if (!isValid && result is not null)
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
                
        /// <summary>
        /// Check Microservice is valid and enabled
        /// </summary>
        /// <param name="matchingRequestMicroserviceDetails"></param>
        /// <returns>return false and bad request if validation fails</returns>
        private (bool isValid, IActionResult? result) ValidateMatchingResults(MatchingEndpoints matchingEndpoint)
        {
            if (!matchingEndpoint.Environment?.Enabled ?? false)
                return (false, BadRequest(ErrorMessageConstants.EnvironmentDisabled));

            if (!matchingEndpoint.Microservice?.Enabled ?? false)
                return (false, BadRequest(ErrorMessageConstants.MicroserviceDisabled));

            return (true, null);
        }
    }
}