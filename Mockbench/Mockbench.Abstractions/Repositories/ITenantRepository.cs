using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Abstractions.Repositories
{
    public interface ITenantRepository
    {
        Task<BaseTenantDto> CreateTenantAsync(BaseTenantDto newTenantDto);
        Task<bool> DeleteTenantAsync(int id);
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync();
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync(int excludingId);
        Task<TenantListDto> GetAllTenantsListAsync(int skip, int take);
        Task<BaseTenantDto> GetTenantByIdAsync(int id);
        Task<BaseTenantDto> GetTenantByNameAsync(string name);
        Task<BaseTenantDto> GetTenantByPathAsync(string path);
        Task<bool> UpdateTenantBaseValuesAsync(BaseTenantDto updatedTenant);
    }
}