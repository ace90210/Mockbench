using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Data.Repositories
{
    public class TenantRepository : BaseRepository, ITenantRepository
    {
        private readonly MockbenchDbContext _context;

        public TenantRepository(MockbenchDbContext context) : base(context)
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
            var tenants = _context.Tenants.Include(t => t.Variables).Where(rt => rt.Id != excludingId).Select(rt => new PathNameItem(rt.Name, rt.Path));

            return await tenants.ToListAsync();
        }

        public async Task<TenantListDto> GetAllTenantsListAsync(int skip, int take)
        {
            var tenants = await _context.Tenants.Include(t => t.Variables).ToListAsync();

            return new TenantListDto()
            {
                TotalTenants = tenants.Count,
                Tenants = tenants.Select(t =>
                {
                    return new TenantBase()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Path = t.Path,
                        SimulateTime = t.SimulateTime
                    };
                }).Skip(skip).Take(take).ToList()
            };
        }

        public async Task<TenantBase?> GetTenantByIdAsync(int id)
        {
            var tenant = await _context.Tenants.Include(t => t.Variables).FirstOrDefaultAsync(t => t.Id == id);

            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime,
                Variables = tenant.Variables.Select(v => new TenantVariableDto()
                {
                    Id = v.Id,
                    Key = v.Key,
                    Value = v.Value
                }).ToList()
            };
        }

        public async Task<TenantBase?> GetTenantByNameAsync(string name)
        {
            var tenant = await _context.Tenants.Include(t => t.Variables).FirstOrDefaultAsync(t => t.Name == name);


            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime,
                Variables = tenant.Variables.Select(v => new TenantVariableDto()
                {
                    Id = v.Id,
                    Key = v.Key,
                    Value = v.Value
                }).ToList()
            };
        }

        public async Task<TenantBase?> GetTenantByPathAsync(string path)
        {
            var tenant = await _context.Tenants.Include(t => t.Variables).FirstOrDefaultAsync(t => t.Path == path.ToLower());

            return tenant == null ? null : new TenantBase()
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Path = tenant.Path,
                SimulateTime = tenant.SimulateTime,
                Variables = tenant.Variables.Select(v => new TenantVariableDto()
                {
                    Id = v.Id,
                    Key = v.Key,
                    Value = v.Value
                }).ToList()
            };
        }

        public async Task<TenantBase> CreateTenantAsync(TenantBase newTenantDto)
        {
            if (newTenantDto == null)
                throw new Exception("No tenant provided");

            if (string.IsNullOrWhiteSpace(newTenantDto.Path))
                throw new Exception("Error path missing or empty");

            var existingTenant = await _context.Tenants.Include(t => t.Variables).FirstOrDefaultAsync(t => t.Path.ToLower() == newTenantDto.Path.ToLower());

            if (existingTenant is not null)
                throw new Exception("tenant with same path already exists. Tenant paths MUST be unique");

            var newTenant = new Tenant()
            {
                Name = newTenantDto.Name,
                Path = newTenantDto.Path.ToLower(),
                SimulateTime = newTenantDto.SimulateTime,
                Variables = newTenantDto.Variables.Select(v => new TenantVariable()
                {
                    Key = v.Key,
                    Value = v.Value
                }).ToList()
            };

            _context.Tenants.Add(newTenant);

            await _context.SaveChangesAsync();

            return new TenantBase()
            {
                Id = newTenant.Id,
                Name = newTenant.Name,
                Path = newTenant.Path,
                SimulateTime = newTenant.SimulateTime,
                Variables = newTenant.Variables.Select(v => new TenantVariableDto()
                {
                    Id = v.Id,
                    Key = v.Key,
                    Value = v.Value
                }).ToList()
            };
        }

        /// <summary>
        /// Update the base properties ONLY on a tenant
        /// </summary>
        /// <param name="updatedTenant">the updated tenant</param>
        /// <returns>true if updated successfully</returns>
        public async Task<bool> UpdateTenantBaseValuesAsync(TenantBase updatedTenant)
        {
            var existingTenant = await _context.Tenants.Include(t => t.Variables).FirstOrDefaultAsync(t => t.Id == updatedTenant.Id);

            if (existingTenant == null)
                return false;

            existingTenant.Name = updatedTenant.Name;
            existingTenant.Path = updatedTenant.Path.ToLower();
            existingTenant.SimulateTime = updatedTenant.SimulateTime;



            if (existingTenant.Variables == null)
            {
                existingTenant.Variables = new List<TenantVariable>();
            }

            if(updatedTenant.Variables == null)
            {
                updatedTenant.Variables = new List<TenantVariableDto>();
            }

            var toAdd = new List<TenantVariableDto>();
            var toRemove = existingTenant.Variables.Where(v => !updatedTenant.Variables.Any(uv => uv.Key == v.Key));

            foreach (var variable in toRemove.ToList())
            {
                existingTenant.Variables.Remove(variable);
            }

            foreach (var variable in updatedTenant.Variables)
            {
                var existingVariable = existingTenant.Variables.FirstOrDefault(v => v.Key == variable.Key);
                if (existingVariable is not null)
                {
                    existingVariable.Value = variable.Value;
                }
                else
                {
                    toAdd.Add(variable);
                }
            }

            foreach(var variable in toAdd)
            {
                existingTenant.Variables.Add(new TenantVariable()
                {
                    Key = variable.Key,
                    Value = variable.Value
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return false;

            _context.Tenants.Remove(tenant);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
