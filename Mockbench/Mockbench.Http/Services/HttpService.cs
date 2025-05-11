using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.Services;
using Mockbench.Services.Helpers;
using Mockbench.Services.Hubs;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.General;
using System.Net;
using System.Net.Sockets;

namespace Mockbench.Server.Services
{
    public class HttpService : IHttpService
    {
        private readonly IMockService _mockService;
        private readonly IProxyService _proxyService;
        private readonly DeploymentConfiguration _deploymentConfiguration;
        private readonly IHubContext<RequestHub> _hubcontext;
        private readonly ILogger<HttpService> _logger;

        public HttpService(IMockService mockService, IProxyService proxyService, IOptions<DeploymentConfiguration> deploymentConfigurationOptions, IHubContext<RequestHub> hubcontext, ILogger<HttpService> logger)
        {
            _mockService = mockService ?? throw new ArgumentNullException(nameof(mockService));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
            _deploymentConfiguration = deploymentConfigurationOptions?.Value ?? throw new ArgumentNullException(nameof(deploymentConfigurationOptions));
            _hubcontext = hubcontext ?? throw new ArgumentNullException(nameof(hubcontext));
            _logger = logger;
        }

        public async Task<IActionResult> ProcessRequestAsync(MatchingEndpoints matchingEndpoints, RestType restType, HttpContext context, string endpointPath)
        {
            if (context == null)
                throw new ArgumentNullException($"Error {nameof(context)} is null");

            RestoreHeaderTypes(context);

            //await SendLiveFeedMessageAsync(context, endpointPath, microservice.Id);

            var foundRequest = await _mockService.GetMatchingEndpointDtoAsync(matchingEndpoints, restType, context, endpointPath);

            bool shouldFallback = false;
            IActionResult proxyResponse = null;

            // If proxy mode is enabled, try to proxy the request first
            if (matchingEndpoints.Microservice != null && matchingEndpoints.Microservice.ProxyMode == ProxyMode.FailOver || matchingEndpoints.Microservice.ProxyMode == ProxyMode.Proxy)
            {
                bool isdown = false;

                try
                {
                    proxyResponse = await _proxyService.ProxyRequestToMicroserviceAsync(matchingEndpoints, restType, context, endpointPath);
                }
                catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
                {
                    // Capture the timeout exception
                    _logger.LogWarning("Proxy request timed out, considering service as down.");
                    isdown = true;

                    proxyResponse = new StatusCodeResult(504);
                }
                catch (HttpRequestException ex) when (ex.InnerException is SocketException)
                {
                    // Capture the timeout exception
                    _logger.LogWarning("Proxy request failed on socket, considering service as down.");
                    isdown = true;

                    proxyResponse = new StatusCodeResult(502);
                }
                catch (HttpRequestException ex)
                {
                    // Capture the timeout exception
                    _logger.LogWarning("Proxy request failed due to HttpRequestException, setting response to 503. Exception: {Exception}", ex);
                    isdown = true;

                    proxyResponse = new StatusCodeResult(503);
                }
                catch (Exception ex)
                {
                    // Log other types of exceptions for troubleshooting
                    _logger.LogError(ex, "An error occurred while proxying the request.");

                    proxyResponse = new ObjectResult(ex.Message) { StatusCode = 500 };
                }

                shouldFallback = matchingEndpoints.Microservice.ProxyMode == ProxyMode.FailOver && (isdown || IsRemoteServiceDown(proxyResponse));

                // If the proxy request succeeded or proxy mode is not failover, return the response
                if (matchingEndpoints.Microservice.ProxyMode == ProxyMode.Proxy)
                {
                    return proxyResponse;
                }

                // If failover mode is enabled and the proxy response failed due to service being down, proceed to mock response
                if (matchingEndpoints.Microservice.ProxyMode == ProxyMode.FailOver)
                {
                    // Log that we are falling back to mock response due to proxy failure
                    _logger.LogWarning("Proxy request failed, falling back to mock response in failover mode.");
                }               
            }

            // If mock mode or simulation time applied
            if (matchingEndpoints.Microservice == null ||
                matchingEndpoints.Microservice.ProxyMode == ProxyMode.None ||
                shouldFallback ||
                HasSimulationApplied(matchingEndpoints, foundRequest))
            {
                // To handle rereading the content better this is handled in the proxy service separately
                // Here we send the message with a fresh httpclient as it will only be sent once
                await SendDebuggerMessageAsync(restType, context, endpointPath);
                if (foundRequest != null)
                {
                    //check auth header
                    if (foundRequest.ExpectAuthHeader && !IsAuthHeaderPresent(context.Request.Headers))
                    {
                        return new UnauthorizedResult();
                    }

                    var existingResponse = await _mockService.GetMockResponseAsync(matchingEndpoints, restType, context, endpointPath);

                    if (existingResponse != null)
                    {
                        var headersToAdd = HttpHelpers.GetResponseHeadersToAdd(matchingEndpoints.Microservice,
                                                                                                 existingResponse.Headers
                                                                                                 .Where(h => h.Name.ToLower() != "host" &&
                                                                                                                         h.Name.ToLower() != "transfer-encoding")
                                                                                                 .Select(h => new HeaderItem(h.Name, string.Join(';', h.Value))));

                        foreach (var headerItem in headersToAdd)
                        {
                            context.Response.Headers.TryAdd(headerItem.Name, string.Join(";", headerItem.Value));
                        }

                        var response = new ContentResult()
                        {
                            Content = existingResponse.Body,
                            ContentType = string.IsNullOrEmpty(existingResponse.ContentType) ? null : existingResponse.ContentType,
                            StatusCode = (int)existingResponse.Code
                        };

                        return ValidatedResponseForKnownIssues(response);
                    }
                }

                // if here we found no mock response so either return null or if in failover return the proxy response instead as has more details
                return matchingEndpoints.Microservice != null && matchingEndpoints.Microservice.ProxyMode == ProxyMode.FailOver ? proxyResponse : null;
            }

            // If none of the above conditions matched, return a proxy response as a fallback
            return await _proxyService.ProxyRequestToMicroserviceAsync(matchingEndpoints, restType, context, endpointPath);
        }

        private static void RestoreHeaderTypes(HttpContext context)
        {
            foreach (var header in context.Request.Headers.Where(h => h.Key.StartsWith(SharedConstants.MockHeaderIsolationPrefix)).ToList())
            {
                // Remove the "Proxied-" prefix and set the header
                var newKey = header.Key.Substring(SharedConstants.MockHeaderIsolationPrefix.Length);
                context.Request.Headers.Remove(header.Key); // Remove the original header
                context.Request.Headers[newKey] = header.Value; // Add the modified header
            }
        }

        private IActionResult ValidatedResponseForKnownIssues(ContentResult response)
        {
            //check known bad responses and return error message instead
            if (response.StatusCode == 204 && !string.IsNullOrEmpty(response.Content))
            {
                //known to exception: returning status code no content with content is bad
                return new ContentResult()
                {
                    Content = $"Response was status code 204 no content but contained the following content. This is not supported:\nOriginal Content Type: {response.ContentType}\nOriginal Body:\n{response.Content}",
                    ContentType = "text/plain",
                    StatusCode = (int)HttpStatusCode.BadGateway
                };
            }

            return response;
        }

        private async Task SendDebuggerMessageAsync(RestType restType, HttpContext context, string endpointPath)
        {
            // Ensure Debug mode is enabled and the DebuggerUrl is set
            if (_deploymentConfiguration?.Debug ?? false && !string.IsNullOrWhiteSpace(_deploymentConfiguration.DebuggerUrl))
            {
                using var httpClient = new HttpClient();
                var requestBody = await GeneralHelpers.RequestBodyToStringAsync(context?.Request);

                try
                {
                    var httpRequestMessage = new HttpRequestMessage
                    {
                        RequestUri = new Uri(_deploymentConfiguration.DebuggerUrl + endpointPath),
                        Content = ConvertHelper.ToExactStringContent(requestBody, context?.Request.ContentType),
                        Method = GetHttpMethod(restType)
                    };

                    if (context?.Request.Headers != null)
                    {
                        foreach (var (key, value) in context.Request.Headers)
                        {
                            httpRequestMessage.Headers.Add(key, value.AsEnumerable());
                        }
                    }

                    await httpClient.SendAsync(httpRequestMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending debugger message: {ex.Message}");
                }
            }
        }

        private HttpMethod GetHttpMethod(RestType restType)
        {
            return restType switch
            {
                RestType.GET => HttpMethod.Get,
                RestType.DELETE => HttpMethod.Delete,
                RestType.POST => HttpMethod.Post,
                RestType.PUT => HttpMethod.Put,
                RestType.PATCH => HttpMethod.Patch,
                _ => throw new ArgumentOutOfRangeException(nameof(restType), restType, "Invalid HTTP method.")
            };
        }

        // TODO fix live feed
        //private async Task SendLiveFeedMessageAsync(HttpContext context, string endpointPath, int microserviceId)
        //{
        //    var request = new HttpRequestDto()
        //    {
        //        Timestamp = DateTime.Now,
        //        HttpMethod = context.Request.Method,
        //        Endpoint = endpointPath,
        //        QueryString = context.Request.QueryString.ToString(),
        //        Body = await GeneralHelpers.RequestBodyToStringAsync(context.Request),
        //        Headers = context.Request.Headers.ToDictionary(
        //            a => a.Key,
        //            a => string.Join(";", a.Value.ToArray())
        //        )
        //    };

        //    await _hubcontext.Clients.All.SendAsync($"{microserviceId}/SendRequest", request);
        //}

        private static bool IsAuthHeaderPresent(IHeaderDictionary headers)
        {
            return headers?.Any(h => h.Key.ToLower().Equals("authorization")) ?? false;
        }

        private bool HasSimulationApplied(MatchingEndpoints matchingEndpoints, EndpointDto endpointDto)
        {
            return matchingEndpoints.Microservice?.SimulateTime != null || matchingEndpoints?.Environment?.SimulateTime != null || matchingEndpoints?.Tenant?.SimulateTime != null || endpointDto?.SimulateTime != null;
        }

        private bool IsRemoteServiceDown(IActionResult proxyResponse)
        {
            if (proxyResponse is ObjectResult objectResult)
            {
                // Check for specific status codes that indicate the service is down
                if (objectResult.StatusCode == 502 || objectResult.StatusCode == 504 || objectResult.StatusCode == 503)
                {
                    return true;
                }
            }
            else if (proxyResponse is StatusCodeResult statusCodeResult)
            {
                // Check for specific status codes that indicate the service is down
                if (statusCodeResult.StatusCode == 502 || statusCodeResult.StatusCode == 504 || statusCodeResult.StatusCode == 503)
                {
                    return true;
                }
            }
            // You can add more conditions here, such as checking response messages or other result types for DNS or timeout issues
            return false;
        }

    }
}
