using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Data.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly MockbenchMainContext _context;

        public TenantRepository(MockbenchMainContext context)
        {
            _context = context;
        }

        public async Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync()
        {
            var tenants = _context.Tenants.Select(rt => new PathNameItem(rt.Name, rt.Path));

            return await tenants.ToListAsync();
        }
        
        public async Task<List<PathNameItem>> GetAllTakenTenantNameAndPathsAsync(int excludingId)
        {
            var tenants = _context.Tenants.Where(rt => rt.ID != excludingId).Select(rt => new PathNameItem(rt.Name, rt.Path));

            return await tenants.ToListAsync();
        }

        public async Task<TenantListDto> GetAllTenantsListAsync(int skip, int take)
        {
            var tenants = await _context.Tenants.ToListAsync();

            return new TenantListDto()
            {
                TotalTenants = tenants.Count,
                Tenants = tenants.Select(t =>
                {
                    return new TenantBase()
                    {
                        Id = t.ID,
                        Name = t.Name,
                        Path = t.Path,
                        SimulateTime = t.SimulateTime
                    };
                }).Skip(skip).Take(take).ToList()
            };
        }

        public async Task<TenantBase?> GetTenantByIdAsync(int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == id);

            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.ID,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime
            };
        }

        public async Task<TenantBase?> GetTenantByNameAsync(string name)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == name);


            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.ID,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime
            };
        }

        public async Task<TenantBase?> GetTenantByPathAsync(string path)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Path == path.ToLower());

            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.ID,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime
            };
        }

        public async Task<TenantBase> CreateTenantAsync(TenantBase newTenantDto)
        {
            if (newTenantDto == null)
                throw new Exception("No tenant provided");

            if (string.IsNullOrWhiteSpace(newTenantDto.Path))
                throw new Exception("Error path missing or empty");

            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Path.ToLower() == newTenantDto.Path.ToLower());

            if (existingTenant != null)
                throw new Exception("tenant with same path already exists. Tenant paths MUST be unique");

            var newTenant = new Tenant()
            {
                Name = newTenantDto.Name,
                Path = newTenantDto.Path.ToLower(),
                SimulateTime = newTenantDto.SimulateTime
            };

            _context.Tenants.Add(newTenant);

            await _context.SaveChangesAsync();

            return new TenantBase()
            {
                Id = newTenant.ID,
                Name = newTenant.Name,
                Path = newTenant.Path,
                SimulateTime = newTenant.SimulateTime
            };
        }

        /// <summary>
        /// Update the base properties ONLY on a tenant
        /// </summary>
        /// <param name="updatedTenant">the updated tenant</param>
        /// <returns>true if updated successfully</returns>
        public async Task<bool> UpdateTenantBaseValuesAsync(TenantBase updatedTenant)
        {
            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == updatedTenant.Id);

            if (existingTenant == null)
                return false;

            existingTenant.Name = updatedTenant.Name;
            existingTenant.Path = updatedTenant.Path.ToLower();
            existingTenant.SimulateTime = updatedTenant.SimulateTime;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == id);

            if (tenant == null)
                return false;

            _context.Tenants.Remove(tenant);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
