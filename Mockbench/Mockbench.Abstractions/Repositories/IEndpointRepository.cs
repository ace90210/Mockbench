using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Abstractions.Repositories
{
    public interface IEndpointRepository: IBaseRepository
    {
        Task AddResponseToEndpointAsync(int endpointId, MockResponseDto mockResponseDto);
        Task<EndpointDto> CreateEndpointAsync(EndpointDto endpointDto);
        Task<bool> DeleteEndpoint(int endpointId);
        Task<MatchingEndpoints> GetAllMatchingEndpointsAsync(string? tenantPath, string? environmentPath, string? microservicePath, string endpointUrl);
        Task<EndpointDto> GetEndpoint(int id);
        Task<UpdateEndpointDto> GetUpdateEndpoint(int id);

        Task<DateTime[]> GetMockResponseTimes(int endpointId);

        Task<EndpointDto> UpdateMockResponses(int endpointId, List<MockResponseDto> responses);
        Task<UpdateEndpointDto> UpdateEndpoint(int endpointId, UpdateEndpointDto endpointDto);
    }
}