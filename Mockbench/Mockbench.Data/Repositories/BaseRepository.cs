using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.Endpoint;

namespace Mockbench.Data.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly MockbenchDbContext _context;

        public BaseRepository(MockbenchDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MatchingEndpoints?> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(MatchingEndpoints matchingEndpoints)
        {
            if (matchingEndpoints == null)
                return null;

            bool changed = false;

            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Path == matchingEndpoints.TenantPath);

            if (matchingEndpoints!.Tenant == null && !string.IsNullOrWhiteSpace(matchingEndpoints.TenantPath))
            {
                if(existingTenant == null)
                {
                    var tenant = new Tenant()
                    {
                        Path = matchingEndpoints.TenantPath,
                        Name = matchingEndpoints.TenantPath
                    };
                    _context.Tenants.Add(tenant);
                    changed = true;

                    existingTenant = tenant;
                }
            }

            var existingEnvironment = await _context.Environments.FirstOrDefaultAsync(e => e.Path == matchingEndpoints.EnvironmentPath);

            if (matchingEndpoints!.Environment == null && !string.IsNullOrWhiteSpace(matchingEndpoints.EnvironmentPath))
            {
                if (existingEnvironment == null)
                {
                    var environment = new Models.Environment()
                    {
                        Path = matchingEndpoints.EnvironmentPath,
                        Name = matchingEndpoints.EnvironmentPath
                    };
                    _context.Environments.Add(environment);
                    changed = true;

                    existingEnvironment = environment;
                }
            }

            var existingMicroservice = await _context.Microservices.FirstOrDefaultAsync(m => m.Path == matchingEndpoints.MicroservicePath);

            if (matchingEndpoints!.Microservice == null && !string.IsNullOrWhiteSpace(matchingEndpoints.MicroservicePath))
            {
                if (existingMicroservice == null)
                {
                    var microservice = new Microservice()
                    {
                        Path = matchingEndpoints.MicroservicePath,
                        Name = matchingEndpoints.MicroservicePath
                    };
                    _context.Microservices.Add(microservice);
                    changed = true;

                    existingMicroservice = microservice;
                }
            }
            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            matchingEndpoints.Tenant = Mapper.Tenant.ToDto(existingTenant);
            matchingEndpoints.Environment = Mapper.Environment.ToDto(existingEnvironment);
            matchingEndpoints.Microservice = Mapper.Microservice.ToDto(existingMicroservice);

            return matchingEndpoints;
        }

        public async Task<MatchingEndpoints?> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(string? tenantPath, string? environmentPath, string? microservicePath)
        {
            bool changed = false;

            var matchingEndpoints = new MatchingEndpoints()
            {
                TenantPath = tenantPath,
                EnvironmentPath = environmentPath,
                MicroservicePath = microservicePath
            };

            var existingTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Path == tenantPath);

            if (!string.IsNullOrWhiteSpace(tenantPath))
            {
                if (existingTenant == null)
                {
                    var tenant = new Tenant()
                    {
                        Path = tenantPath,
                        Name = tenantPath
                    };
                    _context.Tenants.Add(tenant);
                    changed = true;

                    existingTenant = tenant;
                }
            }

            var existingEnvironment = await _context.Environments.FirstOrDefaultAsync(e => e.Path == environmentPath);

            if (!string.IsNullOrWhiteSpace(environmentPath))
            {
                if (existingEnvironment == null)
                {
                    var environment = new Models.Environment()
                    {
                        Path = environmentPath,
                        Name = environmentPath
                    };
                    _context.Environments.Add(environment);
                    changed = true;

                    existingEnvironment = environment;
                }
            }

            var existingMicroservice = await _context.Microservices.FirstOrDefaultAsync(m => m.Path == microservicePath);

            if (!string.IsNullOrWhiteSpace(microservicePath))
            {
                if (existingMicroservice == null)
                {
                    var microservice = new Microservice()
                    {
                        Path = microservicePath,
                        Name = microservicePath
                    };
                    _context.Microservices.Add(microservice);
                    changed = true;

                    existingMicroservice = microservice;
                }
            }
            if (changed)
            {
                await _context.SaveChangesAsync();
            }
            matchingEndpoints.Tenant = Mapper.Tenant.ToDto(existingTenant);
            matchingEndpoints.Environment = Mapper.Environment.ToDto(existingEnvironment);
            matchingEndpoints.Microservice = Mapper.Microservice.ToDto(existingMicroservice);
            return matchingEndpoints;
        }

        public bool ValidateTenantEnvironmentMicroserviceIfNotExists(string? tenantPath, string? environmentPath, string? microservicePath)
        {
            if (!string.IsNullOrWhiteSpace(tenantPath))
            {
                var existingTenant = _context.Tenants.Any(t => t.Path == tenantPath);
                if (!existingTenant) 
                    return false;
            }


            if (!string.IsNullOrWhiteSpace(environmentPath))
            {
                var existingEnvironment = _context.Environments.Any(e => e.Path == environmentPath);
                if (!existingEnvironment)
                    return false;
            }


            if (!string.IsNullOrWhiteSpace(microservicePath))
            {
                var existingMicroservice = _context.Microservices.Any(m => m.Path == microservicePath);
                if (!existingMicroservice)
                    return false;
            }
            return true;
        }
    }
}
