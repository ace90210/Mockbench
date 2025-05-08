using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMicroserviceRepository
    {
        Task<MicroserviceResultDto> CreateMicroservice(MicroserviceResultDto newMicroserviceDto);
        Task<MicroserviceParentIds> GetParentIds(int microserviceId);
        Task<bool> DeleteMicroservice(int id);
        Task<MatchingEndpointMicroserviceDetailsDto> FindMatchingRequest(string tenantPath, string environmentPath, string path);
        Task<MicroserviceResultDto> FindMicroservice(string tenantPath, string environmentPath, string path);
        Task<IEnumerable<MicroserviceResultDto>> GetAllMicroserviceForTenant(int tenantId);
        Task<List<PathNameItem>> GetAllMicroservicePathAndNamesForEnvironment(int environmentId);
        Task<List<PathNameItem>> GetAllMicroservicePathAndNamesForEnvironment(int environmentId, int excludingMicroserviceId);
        Task<IEnumerable<MicroserviceResultDto>> GetAllMicroservices();
        Task<IEnumerable<MicroserviceSearchResultDto>> GetAllMicroserviceSearchResults();
        Task<IEnumerable<MicroserviceResultDto>> GetAllMicroservicesForEnvironment(int environmentId);
        Task<MicroserviceResultDto> GetMicroserviceById(int id);
        Task<MicroserviceResultDto> GetMicroservice(int environmentId, string microservicePath);
        Task<bool> UpdateMicroservice(MicroserviceResultDto updatedMicroservice);
    }
}