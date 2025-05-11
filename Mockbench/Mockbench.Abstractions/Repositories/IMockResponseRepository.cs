using Mockbench.Shared.Models.Response;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMockResponseRepository : IBaseRepository
    {
        Task<(bool success, MockResponseDto? result)> CreateAsync(int endpointId, MockResponseDto response);
        Task<(bool success, List<MockResponseDto> result)> CreateBulkAsync(int endpointId, List<MockResponseDto> responses);
        Task<bool> DeleteAsync(int responseId);
        Task<bool> DeleteBulkAsync(int endpointId, List<MockResponseDto> responses);
        Task<MockResponseDto> GetMockResponseAsync(int id);
        Task<UpdateMockResponseDto> GetUpdateMockResponseAsync(int id);

        Task<UpdateMockResponseDto> PatchMockResponseAsync(int mockResponseId, UpdateMockResponseDto updateMockResponse);
        Task<MockResponseDto> UpdateMockResponseAsync(int mockResponseId, MockResponseDto updatedResponse);
    }
}