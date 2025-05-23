using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Abstractions.Repositories
{
    public interface ITenantRepository : IBaseRepository
    {
        Task<TenantBaseDto> CreateTenantAsync(TenantBaseDto newTenantDto);
        Task<bool> DeleteAsync(int id);
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync();
        Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync(int excludingId);
        Task<TenantListDto> GetAllTenantsListAsync(int skip, int take);
        Task<TenantBaseDto> GetTenantByIdAsync(int id);
        Task<TenantBaseDto> GetTenantByNameAsync(string name);
        Task<TenantBaseDto> GetTenantByPathAsync(string path);
        Task<bool> UpdateTenantBaseValuesAsync(TenantBaseDto updatedTenant);
    }
}