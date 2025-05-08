using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMicroserviceRepository
    {
        Task<MicroserviceResultDto> CreateMicroservice(MicroserviceResultDto newMicroserviceDto);
        Task<bool> DeleteMicroservice(int id);
        Task<MicroserviceResultDto> FindMatchingRequest(string tenantPath, string environmentPath, string path);
        Task<MicroserviceResultDto> FindMicroservice(string tenantPath, string environmentPath, string path);
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames();
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames(int excludingMicroserviceId);
        Task<IEnumerable<MicroserviceResultDto>> GetAllMicroservices();
        Task<IEnumerable<MicroserviceResultDto>> GetAllMicroserviceSearchResults();
        Task<MicroserviceResultDto> GetMicroserviceById(int id);
        Task<MicroserviceResultDto> GetMicroservice(string microservicePath);
        Task<bool> UpdateMicroservice(MicroserviceResultDto updatedMicroservice);
    }
}