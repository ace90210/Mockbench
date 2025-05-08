using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Timetravel;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly MockbenchMainContext _context;
        private readonly DeploymentConfiguration _deploymentConfiguration;
        private readonly TenantMapper _tenantMapper = new TenantMapper();
        private readonly TenantClonerMapper _tenantClonerMapper = new TenantClonerMapper();

        public BaseRepository(MockbenchMainContext context, IOptions<DeploymentConfiguration> deploymentOptions)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _deploymentConfiguration = deploymentOptions?.Value ?? throw new ArgumentNullException(nameof(deploymentOptions));
        }

        #region Set Simulation Time
        public async Task<bool> SetSimulateTimeOnRequest(DateTime? time, int id)
        {
            var endpoint = await _context.Endpoints.FirstOrDefaultAsync(rr => rr.ID == id);

            if (endpoint == null)
                return false;

            endpoint.SimulateTime = time;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetSimulateTimeOnMicroservice(DateTime? time, int id)
        {
            var microservice = await _context.Microservices.FirstOrDefaultAsync(m => m.ID == id);

            if (microservice == null)
                return false;

            microservice.SimulateTime = time;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SetSimulateTimeOnEnvironment(DateTime? time, int id)
        {
            var environment = await _context.Environments.FirstOrDefaultAsync(sg => sg.ID == id);

            if (environment == null)
                return false;

            environment.SimulateTime = time;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SetSimulateTimeOnTenant(DateTime? time, int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == id);

            if (tenant == null)
                return false;

            tenant.SimulateTime = time;

            await _context.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Get Times
        public async Task<TimeTravelDto> GetRequestTimes(int id)
        {
            var endpointDto = await _context.Endpoints.Include(sr => sr.MockResponses).FirstOrDefaultAsync(sr => sr.ID == id);

            if (endpointDto == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var result = endpointDto.MockResponses.Select(rr => rr.CreatedUtc).ToList();


            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = endpointDto.SimulateTime
            };
        }

        public async Task<TimeTravelDto> GetMicroserviceTimes(int id)
        {
            var microservice = await _context.Microservices.FirstOrDefaultAsync(m => m.ID == id);

            if(microservice == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var endpoints = _context.Endpoints.Include(sr => sr.MockResponses).Where(sr => sr.MicroserviceID == id);

            var result = endpoints.SelectMany(sr => sr.MockResponses)
                                            .Select(mr => mr.CreatedUtc).ToList();
            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = microservice.SimulateTime
            };
        }

        public async Task<TimeTravelDto> GetEnvironmentTimes(int id)
        {
            var environment = await _context.Environments.FirstOrDefaultAsync(m => m.ID == id);

            if(environment == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var microservices = _context.Microservices.Include(m => m.Endpoints)
                                                        .ThenInclude(sr => sr.MockResponses)
                                                        .AsSplitQuery()
                                                        .Where(m => m.EnvironmentID == id);

            var result = microservices.SelectMany(m => m.Endpoints)
                                        .SelectMany(sr => sr.MockResponses)
                                        .Select(mr => mr.CreatedUtc).ToList();



            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = environment.SimulateTime
            };
        }

        public async Task<TimeTravelDto> GetTenanEnvironmentTimes(int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == id);

            if (tenant == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var environments = _context.Environments.Include(t => t.Microservices)
                                                        .ThenInclude(m => m.Endpoints)
                                                        .ThenInclude(sr => sr.MockResponses)
                                                        .AsSplitQuery()
                                                        .Where(sg => sg.TenantID == id);

            var result = environments.SelectMany(sg => sg.Microservices)
                                        .SelectMany(m => m.Endpoints)
                                        .SelectMany(sr => sr.MockResponses)
                                        .Select(mr => mr.CreatedUtc).ToList();

            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = tenant.SimulateTime
            };
        }
        #endregion

        public async Task<FullDatabaseDto> ExportDatabaseToJson()
        {
            _tenantClonerMapper.Clone(new Tenant());


            var fullDatabase = new FullDatabaseDto
            {
                DatabaseType = _deploymentConfiguration.DatabaseConfig.Provider.ToString(),
                CodeVersion = SharedConstants.MockbenchVersion,
                AppliedMigrations = await _context.Database.GetAppliedMigrationsAsync()
            };

            var tenants = _context.Tenants.ToList();
            
            foreach (var tenant in tenants)   
            {
                foreach (var environment in tenant.Environments)
                {
                    environment.Microservices = _context.Microservices
                        .Include(ms => ms.Headers)
                        .Where(ms => ms.EnvironmentID == environment.ID)
                        .ToList();
                    
                    foreach (var microservice in environment.Microservices)
                    {
                        microservice.Endpoints = _context.Endpoints
                                                                .Include(sr => sr.EndpointHeaders)
                                                                .Include(sr => sr.QueryParameters)
                                                                .Where(sr => sr.MicroserviceID == microservice.ID)
                                                                .AsSplitQuery()
                                                                .ToList();
                        

                        foreach (var endpoint in microservice.Endpoints)
                        {
                            endpoint.MockResponses = _context.MockResponses
                                                            .Include(mr => mr.Headers)
                                                            .Where(mr => mr.EndpointId == endpoint.ID)
                                                            .ToList();
                            
                            //clear ids to zero
                            endpoint.MockResponses.ForEach(mr =>
                            {
                                mr.ID = 0;
                                mr.Headers?.ForEach(h => h.ID = 0);
                            });
                        }
                        
                        //clear ids to zero
                        microservice.Endpoints.ForEach(mr =>
                        {
                            mr.ID = 0;
                            mr.EndpointHeaders?.ForEach(h => h.ID = 0);
                            mr.QueryParameters?.ForEach(h => h.Id = 0);
                        });
                    }
                    
                    //clear ids to zero
                    environment.Microservices.ForEach(mr =>
                    {
                        mr.ID = 0;
                        mr.Headers?.ForEach(h => h.ID = 0);
                    });
                }
            }
            
            fullDatabase.Tenants = tenants.Select(t =>
            {
                t.ID = 0;
                return _tenantMapper.ToTenantDto(t);
            }).ToList();
            
            return fullDatabase;
        }

        public async Task<bool> ImportDatabase(FullDatabaseDto import, bool skipDuplicateTenants)
        {
            if (import.Tenants == null)
                return false;
            var tenants = import.Tenants.Select(t=> _tenantMapper.ToTenantEntity(t));

            var existingTenant = _context.Tenants;

            foreach (var tenant in tenants)
            {
                if (!existingTenant.Any(et => et.Name.ToUpper().Equals(tenant.Name.ToUpper()) ||
                                                    et.Path.ToUpper().Equals(tenant.Path.ToUpper())
                                        ))
                {
                    _context.Tenants.Add(tenant);
                }
                else if(!skipDuplicateTenants)
                {
                    throw new InvalidOperationException("Cannot import duplicate tenant");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
