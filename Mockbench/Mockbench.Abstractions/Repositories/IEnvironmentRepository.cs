using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.General;

namespace Mockbench.Abstractions.Repositories
{
    public interface IEnvironmentRepository
    {
        Task<BaseEnvironmentDto> CreateEnvironment(BaseEnvironmentDto newEnvironmentDto);
        Task<bool> DeleteEnvironment(int id);
        Task<List<PathNameItem>> GetAllEnvironmentNameAndPathsForTenant(int tenantId);
        Task<List<PathNameItem>> GetAllEnvironmentNameAndPathsForTenant(int tenantId, int excludingEnvironmentId);
        Task<BasicEnvironmentDto> GetEnvironmentById(int id);
        Task<int?> GetEnvironmentId(int tenantId, string environmentPath);
        Task<int?> GetEnvironmentId(string path, string environmentPath);
        Task<IEnumerable<BasicEnvironmentDto>> GetEnvironments();
        Task<EnvironmentOverviewCollection> GetEnvironmentsByTenantId(int id);
        Task<bool> UpdateEnvironmentBaseValues(BaseEnvironmentDto updatedEnvironment);
    }
}