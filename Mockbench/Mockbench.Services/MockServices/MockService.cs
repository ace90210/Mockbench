using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Mockbench.Abstractions.MockServices;
using Mockbench.Abstractions.Repositories;
using Mockbench.Services.Helpers;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Services.MockServices
{
    public class MockService : IMockService
    {
        private static readonly Random RandomNumberGenerator = new();

        private readonly IEndpointRepository _endpointRepository;

        public MockService(IEndpointRepository endpointRepository)
        {
            _endpointRepository = endpointRepository;
        }

        public async Task<EndpointDto> GetMatchingEndpointDtoAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, RestType restType, HttpContext context, string endpointPath)
        {
            string body = restType != RestType.GET ? await GeneralHelpers.RequestBodyToStringAsync(context?.Request) : null;

            return await FindMatchingEndpointAsync(tenant?.Path, environment?.Path, microservice?.Path, context, restType, endpointPath, body);
        }

        public async Task<MockResponseDto> GetMockResponseAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, RestType restType, HttpContext context, string endpointPath)
        {
            DateTime? resolvedSimulateTime = microservice.SimulateTime?.AddMicroseconds(1) ??
                                    environment.SimulateTime?.AddMicroseconds(1) ??
                                    tenant.SimulateTime?.AddMicroseconds(1);

            var matchingRequestDto = await GetMatchingEndpointDtoAsync(tenant, environment, microservice, restType, context, endpointPath);

            MockResponseDto existingResponse = InnerGetMockResponse(matchingRequestDto, microservice.RandomiseMockResult, resolvedSimulateTime);

            if (existingResponse?.FakeDelay > 0)
            {
                await Task.Delay(existingResponse.FakeDelay);
            }
            else if (microservice.FakeDelay > 0)
            {
                await Task.Delay(microservice.FakeDelay);
            }

            return existingResponse;
        }

        public async Task CreateMockResponseIfNotExistAsync(TenantBase tenant, EnvironmentDto environment, FullMicroserviceDto microservice, HttpContext context, RestType restType, string endpointPath, string requestBody, HttpResponseMessage response, TimeSpan latency)
        {
            var matchingEndpoint = await FindMatchingEndpointAsync(tenant?.Path, environment?.Path, microservice?.Path, context, restType, endpointPath, requestBody);

            if (matchingEndpoint == null || matchingEndpoint.MockBehaviour == MockBehaviour.AutoMockWithProxy)
            {
                var mockResponse = await BuildMockResponseAsync(microservice, response, latency);

                if (matchingEndpoint == null)
                {
                    var endpoint = BuildMockServiceRequestUsingResponse(microservice, context, restType, endpointPath, requestBody, mockResponse);

                    await _endpointRepository.CreateEndpointAsync(microservice.Id, endpoint);
                }
                else if (!DoesResponseExist(matchingEndpoint, mockResponse))
                {
                    // no responses found add to requests list of responses
                    await _endpointRepository.AddResponseToEndpointAsync(matchingEndpoint.Id, mockResponse);
                }
            }
        }

        private static async Task<MockResponseDto> BuildMockResponseAsync(FullMicroserviceDto microservice, HttpResponseMessage response, TimeSpan latency)
        {
            if (response == null)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync()!;
            var responseContentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

            var mockResponse = new MockResponseDto
            {
                Body = responseContent,
                ContentType = responseContentType,
                Encoding = SupportedEncodingType.UTF8,
                Latency = latency,
                Checksum = ChecksumHelpers.CreateChecksum(SupportedEncodingType.UTF8, $"{responseContent}-{response.StatusCode}-{responseContentType}"),
                Code = response.StatusCode
            };

            mockResponse.Headers = GetResponseHeaders(microservice, response);

            return mockResponse;
        }

        private static EndpointDto BuildMockServiceRequestUsingResponse(FullMicroserviceDto microservice, HttpContext context, RestType restType, string endpointPath, string requestBody, MockResponseDto newResponse)
        {
            var queryParams = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());

            //no request so create new request for the provided response
            var endpoint = new EndpointDto()
            {
                MicroserviceId = microservice.Id,
                ExactUrlMatch = true,
                FromBody = requestBody,
                FromUrl = endpointPath,
                RestType = restType,
                MockResponses = new List<MockResponseDto>() { newResponse },
                Enabled = true,
                EndpointHeaders = GetRequestHeaders(microservice, context),
                ExpectAuthHeader = context.Request.Headers.Any(h => h.Key.ToLower() == "authorization"),
                QueryParameters = queryParams.AllKeys.Select((k, i) => new QueryParameterDto() { Name = k, Value = queryParams[k], OrderIndex = i }).ToList()
            };

            if (microservice.Headers != null && microservice.Headers.Count > 0)
            {
                endpoint.ExpectAuthHeader = endpoint.EndpointHeaders?.Any(h => h.Name.ToLower() == "authorization") ?? false;
            }

            return endpoint;
        }

        private static List<EndpointHeaderDto> GetRequestHeaders(FullMicroserviceDto microservice, HttpContext context)
        {
            var serviceHeaders = new List<EndpointHeaderDto>();

            foreach (var header in context.Request.Headers.Where(h => h.Key.ToLower() != "host"))
            {
                if (header.Value != default(StringValues))
                {
                    switch (microservice.HeadersMode)
                    {
                        case HeadersMode.All:
                            {
                                serviceHeaders.Add(new EndpointHeaderDto() { Name = header.Key, Value = header.Value });
                            }
                            break;
                        case HeadersMode.UserDefined:
                            {
                                var matchingHeader = microservice.Headers?.Any(h => h.Enabled && h.Outgoing && h.Name.ToLower() == header.Key.ToLower());
                                if (matchingHeader ?? false)
                                {
                                    serviceHeaders.Add(new EndpointHeaderDto() { Name = header.Key, Value = header.Value });
                                }
                            }
                            break;
                    }
                }
            }
            return serviceHeaders;
        }

        private static List<MockResponseHeaderDto> GetResponseHeaders(FullMicroserviceDto microservice, HttpResponseMessage response)
        {
            var responseHeaders = new List<MockResponseHeaderDto>();
            foreach (var header in response.Headers.Where(h => h.Key.ToLower() != "host"))
            {
                if (header.Value != default(StringValues))
                {
                    var headersMode = microservice?.HeadersMode ?? HeadersMode.All;
                    switch (headersMode)
                    {
                        case HeadersMode.All:
                            {
                                responseHeaders.Add(new MockResponseHeaderDto() { Name = header.Key, Value = string.Join(';', header.Value) });
                            }
                            break;
                        case HeadersMode.UserDefined:
                            {
                                var matchingHeader = microservice?.Headers?.Any(h => h.Enabled && h.Incoming && h.Name.ToLower() == header.Key.ToLower());
                                if (matchingHeader ?? false)
                                {
                                    responseHeaders.Add(new MockResponseHeaderDto() { Name = header.Key, Value = string.Join(';', header.Value) });
                                }
                            }
                            break;
                    }
                }
            }

            return responseHeaders;
        }

        public async Task<EndpointDto> FindMatchingEndpointAsync(string? tenantPath, string? environmentPath, string? microservicePath, HttpContext context, RestType restType, string endpointUrl, string requestBody)
        {
            var allMicroserviceEndpoints = await _endpointRepository.GetAllMatchingEndpointsAsync(tenantPath, environmentPath, microservicePath, endpointUrl);

            foreach (var endpoint in allMicroserviceEndpoints.Where(mc => mc.RestType == restType))
            {
                if (CompareRequest($"{endpointUrl}", requestBody, endpoint, context.Request.QueryString))
                {
                    return endpoint;
                }
            }

            Console.WriteLine($"No endpoints found for\tEnd point: {endpointUrl}\tBody: {requestBody}");
            return null;
        }

        private MockResponseDto InnerGetMockResponse(EndpointDto endpoint, bool pickRandom, DateTime? simulateTime)
        {
            if (endpoint != null && endpoint.MockBehaviour != MockBehaviour.ProxyOnly)
            {
                var resolvedSimulateTime = endpoint.SimulateTime?.AddMicroseconds(1) ?? simulateTime?.AddMicroseconds(1);

                Console.WriteLine($"Endpoint match found: {endpoint.Id}");

                var enabledResponses = endpoint.MockResponses.Where(r => r.Enabled)
                                                                               .Where(er => resolvedSimulateTime == null || er.CreatedUtc < resolvedSimulateTime)
                                                                               .ToList();

                var filteredAndOrderedResponses = enabledResponses.OrderBy(er => er.Priority).ThenByDescending(er => er.CreatedUtc).ToList();

                if (filteredAndOrderedResponses.Count > 0)
                {
                    if (!pickRandom || resolvedSimulateTime != null)
                    {
                        return filteredAndOrderedResponses.First();
                    }

                    return filteredAndOrderedResponses[RandomNumberGenerator.Next(enabledResponses.Count())];
                }
            }

            return null;
        }

        private static bool CompareRequest(string endpointUrl, string requestBody, EndpointDto endpoint, QueryString queryString)
        {
            bool bodyMatches = requestBody == null || requestBody.ToLower().Equals(endpoint.FromBody?.ToLower()) || (string.IsNullOrEmpty(requestBody) && string.IsNullOrEmpty(endpoint.FromBody));

            bool queryParametersMatch = false;

            if (!queryString.HasValue && endpoint.QueryParameters?.Count == 0)
            {
                queryParametersMatch = true;
            }
            else if (queryString.HasValue && endpoint.QueryParameters?.Count > 0)
            {
                queryParametersMatch = CompareQueryParameters(queryString, endpoint.QueryParameters);
            }

            // Exact match check
            if (endpoint.ExactUrlMatch &&
                endpoint.FromUrl.ToLower().Equals(endpointUrl.ToLower()) &&
                bodyMatches)
            {
                return queryParametersMatch;
            }

            // starts with check
            if (!endpoint.ExactUrlMatch && endpointUrl.ToLower().StartsWith(endpoint.FromUrl.ToLower()) && bodyMatches)
            {
                return true; // dont check query parameters if disable exact match as none sensical to match query parameters for different end points
            }

            return false;
        }

        private static bool CompareQueryParameters(QueryString queryString, List<QueryParameterDto> queryParameters)
        {
            var requestQueryParameters = queryString.Value.TrimStart('?').Split("&").ToHashSet();

            if (requestQueryParameters.Count != queryParameters.Count)
            {
                return false;
            }

            foreach (var queryParameter in queryParameters.Where(qp => !qp.Ignore))
            {
                if (!requestQueryParameters.Contains($"{queryParameter.Name}={queryParameter.Value}"))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DoesResponseExist(EndpointDto endpoint, MockResponseDto newResponse)
        {
            return endpoint.MockResponses.Any(rr => newResponse.Checksum.Equals(rr.Checksum));
        }
    }
}
