using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMicroserviceRepository : IBaseRepository
    {
        Task<MicroserviceResultDto> CreateMicroservice(MicroserviceResultDto newMicroserviceDto);
        Task<bool> DeleteMicroservice(int id);
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