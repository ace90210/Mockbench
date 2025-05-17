using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Timetravel;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Repositories
{
    public class CommonRepository : BaseRepository, ICommonRepository
    {
        private readonly MockbenchDbContext _context;
        private readonly DeploymentConfiguration _deploymentConfiguration;

        public CommonRepository(MockbenchDbContext context, IOptions<DeploymentConfiguration> deploymentOptions) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _deploymentConfiguration = deploymentOptions?.Value ?? throw new ArgumentNullException(nameof(deploymentOptions));
        }

        #region Set Simulation Time
        public async Task<bool> SetSimulateTimeOnRequest(DateTime? time, int id)
        {
            var endpoint = await _context.Endpoints.FirstOrDefaultAsync(rr => rr.Id == id);

            if (endpoint == null)
                return false;

            endpoint.SimulateTime = time;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetSimulateTimeOnMicroservice(DateTime? time, int id)
        {
            var microservice = await _context.Microservices.FirstOrDefaultAsync(m => m.Id == id);

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
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

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
            var endpointDto = await _context.Endpoints.Include(sr => sr.MockResponses).FirstOrDefaultAsync(sr => sr.Id == id);

            if (endpointDto == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var result = endpointDto.MockResponses.Select(rr => rr.CreatedUtc!.Value).ToList();


            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = endpointDto.SimulateTime
            };
        }

        public async Task<TimeTravelDto> GetMicroserviceTimes(int id)
        {
            var microservice = await _context.Microservices.FirstOrDefaultAsync(m => m.Id == id);

            if(microservice == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var endpoints = _context.Endpoints.Include(sr => sr.MockResponses).Where(sr => sr.MicroserviceId == id);

            var result = endpoints.SelectMany(sr => sr.MockResponses)
                                            .Select(mr => mr.CreatedUtc!.Value).ToList();
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
                                                        .AsSplitQuery();

            var result = microservices.SelectMany(m => m.Endpoints)
                                        .SelectMany(sr => sr.MockResponses)
                                        .Select(mr => mr.CreatedUtc!.Value).ToList();

            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = environment.SimulateTime
            };
        }

        public async Task<TimeTravelDto> GetTenanEnvironmentTimes(int id)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
            {
                return new TimeTravelDto()
                {
                    AvailableTimes = new List<DateTime>(),
                    CurrentTime = null
                };
            }

            var result = _context.Microservices
                                        .SelectMany(m => m.Endpoints)
                                        .SelectMany(sr => sr.MockResponses)
                                        .Select(mr => mr.CreatedUtc!.Value).ToList();

            return new TimeTravelDto()
            {
                AvailableTimes = result.ToList(),
                CurrentTime = tenant.SimulateTime
            };
        }
        #endregion

        public async Task<FullDatabaseDto> ExportDatabaseToJson()
        {
            Mapper.TenantCloner.Clone(new Tenant());


            var fullDatabase = new FullDatabaseDto
            {
                DatabaseType = _deploymentConfiguration.DatabaseConfig.Provider.ToString(),
                CodeVersion = SharedConstants.MockbenchVersion,
                AppliedMigrations = await _context.Database.GetAppliedMigrationsAsync()
            };

            var tenants = _context.Tenants.ToList();

            tenants.ForEach(t =>
            {
                t.Id = 0;
                t.Variables.ForEach(v =>
                {
                    v.Id = 0;
                });
            });

            var environments = _context.Environments.ToList();

            environments.ForEach(e =>
            {
                e.ID = 0;
                e.Variables.ForEach(v =>
                {
                    v.Id = 0;
                });
            });

            fullDatabase.Tenants = Mapper.Tenant.ToDtos(tenants);

            fullDatabase.Environments = Mapper.Environment.ToDtos(environments);

            var microservices = _context.Microservices
                        .Include(ms => ms.Headers)
                        .Include(ms => ms.Endpoints)
                        .ToList();

            fullDatabase.Microservices = Mapper.Microservice.ToDtos(microservices);

                    
            foreach (var microservice in microservices)
            {
                microservice.Endpoints = _context.Endpoints
                                                        .Include(sr => sr.EndpointHeaders)
                                                        .Include(sr => sr.QueryParameters)
                                                        .Where(sr => sr.MicroserviceId == microservice.Id)
                                                        .AsSplitQuery()
                                                        .ToList();
                        

                foreach (var endpoint in microservice.Endpoints)
                {
                    endpoint.MockResponses = _context.MockResponses
                                                    .Include(mr => mr.Headers)
                                                    .Where(mr => mr.EndpointId == endpoint.Id)
                                                    .ToList();
                            
                    //clear ids to zero
                    endpoint.MockResponses.ForEach(mr =>
                    {
                        mr.Id = 0;
                        mr.Headers?.ForEach(h => h.Id = 0);
                    });
                }
                        
                //clear ids to zero
                microservice.Endpoints.ForEach(mr =>
                {
                    mr.Id = 0;
                    mr.EndpointHeaders?.ForEach(h => h.Id = 0);
                    mr.QueryParameters?.ForEach(h => h.Id = 0);
                });
            }
                    
            //clear ids to zero
            microservices.ForEach(mr =>
            {
                mr.Id = 0;
                mr.Headers?.ForEach(h => h.Id = 0);
                mr.Endpoints?.ForEach(e => e.Id = 0);
            });            
            
            return fullDatabase;
        }

        public async Task<bool> ImportDatabase(FullDatabaseDto import, bool skipDuplicates)
        {
            if (import.Tenants == null)
                import.Tenants = new List<TenantBase>();

            if (import.Environments == null)
                import.Environments = new List<EnvironmentDto>();


            if (import.Microservices == null)
                import.Microservices = new List<MicroserviceDto>();

            var tenants = Mapper.Tenant.ToEntities(import.Tenants.ToList());
            var environments = Mapper.Environment.ToEntities(import.Environments.ToList());
            var microservices = Mapper.Microservice.ToEntities(import.Microservices.ToList());

            var existingTenants = _context.Tenants;

            foreach (var tenant in tenants)
            {
                if (!existingTenants.Any(et => et.Name.ToUpper().Equals(tenant.Name.ToUpper()) ||
                                                    et.Path.ToUpper().Equals(tenant.Path.ToUpper())
                                        ))
                {
                    _context.Tenants.Add(tenant);
                }
                else if (!skipDuplicates)
                {
                    throw new InvalidOperationException("Cannot import duplicate tenant");
                }
            }

            var existingEnvironments = _context.Environments;

            foreach (var environment in environments)
            {
                if (!existingEnvironments.Any(e => e.Name.ToUpper().Equals(environment.Name.ToUpper()) ||
                                                    e.Path.ToUpper().Equals(environment.Path.ToUpper())
                                        ))
                {
                    _context.Environments.Add(environment);
                }
                else if (!skipDuplicates)
                {
                    throw new InvalidOperationException("Cannot import duplicate environment");
                }
            }

            var existingMicroservices = _context.Microservices;

            foreach (var microservice in microservices)
            {
                if (!existingMicroservices.Any(em => em.Name.ToUpper().Equals(microservice.Name.ToUpper()) ||
                                                    em.Path.ToUpper().Equals(microservice.Path.ToUpper())
                                        ))
                {
                    _context.Microservices.Add(microservice);
                }
                else if (!skipDuplicates)
                {
                    throw new InvalidOperationException("Cannot import duplicate microservice");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
