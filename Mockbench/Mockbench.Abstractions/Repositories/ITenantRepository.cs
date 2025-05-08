using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Abstractions.Repositories
{
    public interface ITenantRepository
    {
        Task<TenantBase> CreateTenantAsync(TenantBase newTenantDto);
        Task<bool> DeleteTenantAsync(int id);
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync();
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync(int excludingId);
        Task<TenantListDto> GetAllTenantsListAsync(int skip, int take);
        Task<TenantBase> GetTenantByIdAsync(int id);
        Task<TenantBase> GetTenantByNameAsync(string name);
        Task<TenantBase> GetTenantByPathAsync(string path);
        Task<bool> UpdateTenantBaseValuesAsync(TenantBase updatedTenant);
    }
}