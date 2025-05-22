using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Data.Mappers
{
    public static class EndpointMappers
    {
        public static Endpoint? ToEntity(this EndpointDto endpointDto, bool createNew)
        {
            return endpointDto == null ? null : new Endpoint()
            {
                FromBody = endpointDto.FromBody,
                FromUrl = endpointDto.FromUrl,
                ExactUrlMatch = endpointDto.ExactUrlMatch,
                ExpectAuthHeader = endpointDto.ExpectAuthHeader,
                MockBehaviour = endpointDto.MockBehaviour,
                Enabled = endpointDto.Enabled,
                TenantId = endpointDto.TenantId,
                EnvironmentId = endpointDto.EnvironmentId,
                MicroserviceId = endpointDto.MicroserviceId,
                CreatedUtc = !createNew ? endpointDto.CreatedUtc : DateTime.Now,
                RestType = (Shared.Models.Enum.RestType)endpointDto.RestType,
                SimulateTime = endpointDto.SimulateTime,
                MockResponses = endpointDto.MockResponses?.ToEntities(),
                QueryParameters = endpointDto.QueryParameters?.ToEntities(),
                EndpointHeaders = endpointDto.EndpointHeaders?.ToEntities()
            };
        }

        public static List<Endpoint> ToEntities(this List<EndpointDto> endpointDtos, bool createNew)
        {
            return endpointDtos?.Select(endpointDto => endpointDto.ToEntity(createNew)).ToList();
        }

        public static Endpoint? ToEntity(this UpdateEndpointDto endpointDto)
        {
            return endpointDto == null ? null : new Endpoint()
            {
                FromBody = endpointDto.FromBody,
                FromUrl = endpointDto.FromUrl,
                ExactUrlMatch = endpointDto.ExactUrlMatch,
                ExpectAuthHeader = endpointDto.ExpectAuthHeader,
                MockBehaviour = endpointDto.MockBehaviour,
                Enabled = endpointDto.Enabled,
                RestType = endpointDto.RestType,
                SimulateTime = endpointDto.SimulateTime,
                MockResponses = endpointDto.Responses?.ToEntities(),
                QueryParameters = endpointDto.QueryParameters?.ToEntities(),
                EndpointHeaders = endpointDto.EndpointHeaders?.ToEntities(),
                CreatedUtc = endpointDto.CreatedUtc ?? DateTime.UtcNow
        };
        }

        public static List<Endpoint> ToEntities(this List<UpdateEndpointDto> endpointDtos)
        {
            return endpointDtos?.Select(endpointDto => endpointDto.ToEntity()).ToList();
        }

        public static EndpointDto? ToDto(this Endpoint endpoint, bool createNew)
        {
            return endpoint == null ? null : new EndpointDto()
            {
                Id = endpoint.Id,
                FromBody = endpoint.FromBody,
                FromUrl = endpoint.FromUrl,
                ExactUrlMatch = endpoint.ExactUrlMatch,
                ExpectAuthHeader = endpoint.ExpectAuthHeader,
                MockBehaviour = endpoint.MockBehaviour,
                Enabled = endpoint.Enabled,
                TenantId = endpoint.TenantId,
                EnvironmentId = endpoint.EnvironmentId,
                MicroserviceId = endpoint.MicroserviceId,
                CreatedUtc = !createNew ? endpoint.CreatedUtc : DateTime.Now,
                RestType = endpoint.RestType,
                SimulateTime = endpoint.SimulateTime,
                MockResponses = endpoint.MockResponses?.ToDtos(),
                QueryParameters = endpoint.QueryParameters?.ToDtos(),
                EndpointHeaders = endpoint.EndpointHeaders?.ToDtos()
            };
        }


        public static List<EndpointDto> ToDtos(this List<Endpoint> endpoints, bool createNew)
        {
            return endpoints?.Select(endpoint => endpoint.ToDto(createNew)).ToList();
        }

        public static UpdateEndpointDto? ToUpdateDto(this Endpoint endpoint)
        {
            return endpoint == null ? null : new UpdateEndpointDto()
            {
                FromBody = endpoint.FromBody,
                FromUrl = endpoint.FromUrl,
                ExactUrlMatch = endpoint.ExactUrlMatch,
                ExpectAuthHeader = endpoint.ExpectAuthHeader,
                MockBehaviour = endpoint.MockBehaviour,
                Enabled = endpoint.Enabled,
                RestType = endpoint.RestType,
                SimulateTime = endpoint.SimulateTime,
                Responses = endpoint.MockResponses?.ToDtos(),
                QueryParameters = endpoint.QueryParameters?.ToDtos(),
                EndpointHeaders = endpoint.EndpointHeaders?.ToDtos(),
                CreatedUtc = endpoint.CreatedUtc
            };
        }

        public static List<UpdateEndpointDto> ToUpdateDtos(this List<Endpoint> endpoints)
        {
            return endpoints?.Select(endpoint => endpoint.ToUpdateDto()).ToList();
        }

        public static UpdateEndpointDto? ToUpdateDto(this EndpointDto endpoint)
        {
            return endpoint == null ? null : new UpdateEndpointDto()
            {
                FromBody = endpoint.FromBody,
                FromUrl = endpoint.FromUrl,
                ExactUrlMatch = endpoint.ExactUrlMatch,
                ExpectAuthHeader = endpoint.ExpectAuthHeader,
                MockBehaviour = endpoint.MockBehaviour,
                Enabled = endpoint.Enabled,
                RestType = (Shared.Models.Enum.RestType)endpoint.RestType,
                SimulateTime = endpoint.SimulateTime,
                Responses = endpoint.MockResponses,
                QueryParameters = endpoint.QueryParameters,
                EndpointHeaders = endpoint.EndpointHeaders,
                CreatedUtc = endpoint.CreatedUtc
            };
        }

        public static List<UpdateEndpointDto> ToUpdateDtos(this List<EndpointDto> endpoints)
        {
            return endpoints?.Select(endpoint => endpoint.ToUpdateDto()).ToList();
        }

        public static Endpoint UpdateWithDto(this Endpoint baseEndpoint, UpdateEndpointDto endpointDto)
        {
            if (baseEndpoint == null)
                return null;

            baseEndpoint.FromBody = endpointDto.FromBody;
            baseEndpoint.FromUrl = endpointDto.FromUrl;
            baseEndpoint.ExactUrlMatch = endpointDto.ExactUrlMatch;
            baseEndpoint.ExpectAuthHeader = endpointDto.ExpectAuthHeader;
            baseEndpoint.MockBehaviour = endpointDto.MockBehaviour;
            baseEndpoint.Enabled = endpointDto.Enabled;
            baseEndpoint.RestType = endpointDto.RestType;
            baseEndpoint.SimulateTime = endpointDto.SimulateTime;
            baseEndpoint.CreatedUtc = endpointDto.CreatedUtc ?? DateTime.UtcNow;

            MergeResponses(baseEndpoint, endpointDto.Responses);
            MergeQueryParameters(baseEndpoint, endpointDto.QueryParameters);
            MergeRequestHeaders(baseEndpoint, endpointDto.EndpointHeaders);
            
            return baseEndpoint;
        }

        public static Endpoint MergeResponses(this Endpoint baseEndpoint, List<MockResponseDto> responses)
        {
            var responsesToAdd = responses.Where(sr => (baseEndpoint.MockResponses?.All(rr => sr.Id != rr.Id) ?? false)|| sr.Id == 0).ToList();
            var responsesToUpdate = baseEndpoint.MockResponses.Where(rr => responses.Any(sr => sr.Id == rr.Id && sr.Id > 0));

            baseEndpoint.MockResponses.RemoveAll(rr => !responses.Any(sr => sr.Id == rr.Id && sr.Id > 0));

            if (responsesToAdd is not null && responsesToAdd.Any())
            {
                baseEndpoint.MockResponses.AddRange(responsesToAdd.ToEntities());
            }

            foreach(var baseResponse in responsesToUpdate)
            {
                var updatedResponse = responses.First(r => r.Id == baseResponse.Id);
                baseResponse.Body = updatedResponse.Body;
                baseResponse.Encoding = updatedResponse.Encoding;
                baseResponse.StatusCode = updatedResponse.StatusCode;
                baseResponse.CreatedUtc = updatedResponse.CreatedUtc;

                MergeResponseHeaders(baseResponse, updatedResponse.Headers);
            }
            
            
            return baseEndpoint;
        }

        public static void MergeQueryParameters(this Endpoint baseEndpoint, List<QueryParameterDto> queryParameters)
        {
            var querysToAdd = queryParameters.Where(sr => (baseEndpoint.QueryParameters?.All(qp => sr.Id != qp.Id) ?? false)|| sr.Id == 0).ToList();
            var querysToUpdate = baseEndpoint.QueryParameters.Where(qp => queryParameters.Any(sr => sr.Id == qp.Id && sr.Id > 0));

            baseEndpoint.QueryParameters.RemoveAll(qp => !queryParameters.Any(sr => sr.Id == qp.Id && sr.Id > 0));

            if (querysToAdd.Any())
            {
                baseEndpoint.QueryParameters.AddRange(querysToAdd.ToEntities());
            }

            foreach(var baseQueryParameter in querysToUpdate)
            {
                var updatedQueries = queryParameters.First(qp => qp.Id == baseQueryParameter.Id);
                baseQueryParameter.Name = updatedQueries.Name;
                baseQueryParameter.Value = updatedQueries.Value;
                baseQueryParameter.Ignore = updatedQueries.Ignore;
                baseQueryParameter.OrderIndex = updatedQueries.OrderIndex;
            }
        }
        
        public static void MergeRequestHeaders(this Endpoint baseEndpoint, List<EndpointHeaderDto> headers)
        {
            if (baseEndpoint.EndpointHeaders == null)
                baseEndpoint.EndpointHeaders = new List<EndpointHeader>();

            if (headers == null)
                headers = new List<EndpointHeaderDto>();
            
            var headersToAdd = headers.Where(h => baseEndpoint.EndpointHeaders.All(rr => h.Id != rr.Id)|| h.Id == 0).ToList();
            var headersToUpdate = baseEndpoint.EndpointHeaders.Where(rr => headers.Any(h => h.Id == rr.Id && h.Id > 0));

            baseEndpoint.EndpointHeaders.RemoveAll(rr => !headers.Any(sr => sr.Id == rr.Id && sr.Id > 0));

            if (headersToAdd.Any())
            {
                baseEndpoint.EndpointHeaders.AddRange(headersToAdd.ToEntities());
            }

            foreach(var baseHeader in headersToUpdate)
            {
                var updatedHeader = headers.First(h => h.Id == baseHeader.Id);
                baseHeader.Name = updatedHeader.Name;
                baseHeader.Value = string.Join(';', updatedHeader.Value);
            }
        }
        
        public static void MergeResponseHeaders(this MockResponse baseMockResponse, List<MockResponseHeaderDto> headers)
        {
            if (baseMockResponse.Headers == null)
                baseMockResponse.Headers = new List<ResponseHeader>();

            if (headers == null)
                headers = new List<MockResponseHeaderDto>();
            
            var headersToAdd = headers.Where(h => baseMockResponse.Headers.All(rr => h.Id != rr.Id)|| h.Id == 0).ToList();
            var headersToUpdate = baseMockResponse.Headers.Where(rr => headers.Any(h => h.Id == rr.Id && h.Id > 0));

            baseMockResponse.Headers.RemoveAll(rr => !headers.Any(sr => sr.Id == rr.Id && sr.Id > 0));

            if (headersToAdd.Any())
            {
                baseMockResponse.Headers.AddRange(headersToAdd.ToEntities());
            }

            foreach(var baseHeader in headersToUpdate)
            {
                var updatedHeader = headers.First(h => h.Id == baseHeader.Id);
                baseHeader.Name = updatedHeader.Name;
                baseHeader.Value = string.Join(';', updatedHeader.Value);
            }
        }
    }
}
