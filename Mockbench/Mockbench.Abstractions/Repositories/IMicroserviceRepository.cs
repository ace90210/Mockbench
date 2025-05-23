using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Abstractions.Repositories
{
    public interface IMicroserviceRepository : IBaseRepository
    {
        Task<MicroserviceDto> CreateMicroserviceAsync(MicroserviceDto newMicroserviceDto);
        Task<bool> DeleteMicroserviceAsync(int id);
        Task<MicroserviceDto> FindMicroservice(string tenantPath, string environmentPath, string path);
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames();
        Task<List<PathNameItem>> GetAllMicroservicePathAndNames(int excludingMicroserviceId);
        Task<IEnumerable<MicroserviceDto>> GetMicroservicesAsync();
        Task<MicroserviceDto> GetMicroserviceByIdAsync(int id);
        Task<MicroserviceDto> GetMicroservice(string microservicePath);
        Task<bool> UpdateMicroserviceAsync(int id, MicroserviceDto updatedMicroservice);
    }
}