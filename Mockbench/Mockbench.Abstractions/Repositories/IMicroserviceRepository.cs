using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMicroserviceRepository : IBaseRepository
    {
        Task<MicroserviceDto> CreateMicroservice(MicroserviceDto newMicroserviceDto);
        Task<bool> DeleteMicroservice(int id);
        Task<MicroserviceDto> FindMicroservice(string tenantPath, string environmentPath, string path);
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames();
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames(int excludingMicroserviceId);
        Task<IEnumerable<MicroserviceDto>> GetAllMicroservices();
        Task<IEnumerable<MicroserviceDto>> GetAllMicroserviceSearchResults();
        Task<MicroserviceDto> GetMicroserviceById(int id);
        Task<MicroserviceDto> GetMicroservice(string microservicePath);
        Task<bool> UpdateMicroservice(int id, MicroserviceDto updatedMicroservice);
    }
}