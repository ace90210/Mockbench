using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Data.Repositories
{
    public class EnvironmentRepository : IEnvironmentRepository
    {
        private readonly MockbenchMainContext _context;

        public EnvironmentRepository(MockbenchMainContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BasicEnvironmentDto>> GetEnvironments()
        {
            var services = await _context.Environments.Include(sg => sg.Tenant).ToListAsync();

            return services.Select(sg =>
                           new BasicEnvironmentDto()
                           {
                               Id = sg.ID,
                               Name = sg.Name,
                               Enabled = sg.Enabled,
                               Path = $"{sg.Path}",
                               TenantId = sg.TenantID,
                               TenantName = sg.Tenant.Name,
                               DefaultHealthCheckUrl = sg.DefaultHealthCheckUrl,
                               Microservices = GetMicroservicesForEnvironment(sg.ID),
                               SimulateTime = sg.SimulateTime
                           }
                       );
        }

        public async Task<List<PathNameItem>> GetAllEnvironmentNameAndPathsForTenant(int tenantId)
        {
            var environmentPaths = _context.Environments.Where(sg => sg.TenantID == tenantId)
                .Select(sg => new PathNameItem(sg.Name, sg.Path));

            return await environmentPaths.ToListAsync();
        }

        public async Task<List<PathNameItem>> GetAllEnvironmentNameAndPathsForTenant(int tenantId, int excludingServiceId)
        {
            var environmentPaths = _context.Environments.Where(sg => sg.TenantID == tenantId && sg.ID != excludingServiceId)
                .Select(sg => new PathNameItem(sg.Name, sg.Path));

            return await environmentPaths.ToListAsync();
        }

        public async Task<EnvironmentOverviewCollection> GetEnvironmentsByTenantId(int id)
        {
            var tenant = await _context.Tenants.Include(t => t.Environments).FirstOrDefaultAsync(sg => sg.ID == id);

            if (tenant == null)
            {
                return null;
            }

            if (tenant.Environments?.Count == 0)
            {
                return new EnvironmentOverviewCollection()
                {
                    TenantId = id,
                    TenantName = tenant.Name,
                    Environments = new List<BasicEnvironmentDto>()
                };
            }


            return new EnvironmentOverviewCollection()
            {
                TenantId = id,
                TenantName = tenant.Name,
                Environments = tenant.Environments?.Select(sg =>
                            new BasicEnvironmentDto()
                            {
                                Id = sg.ID,
                                Name = sg.Name,
                                Enabled = sg.Enabled,
                                Path = $"{sg.Path}",
                                TenantId = sg.TenantID,
                                TenantName = sg.Tenant.Name,
                                DefaultHealthCheckUrl = sg.DefaultHealthCheckUrl,
                                Microservices = GetMicroservicesForEnvironment(sg.ID),
                                SimulateTime = sg.SimulateTime
                            }
                        ).ToList()
            };
        }

        public async Task<BasicEnvironmentDto> GetEnvironmentById(int id)
        {
            var environment = await _context.Environments
                                        .Include(sg => sg.Tenant)
                                        .Include(sg => sg.Microservices)
                                        .ThenInclude(ms => ms.Endpoints)
                                        .ThenInclude(sr => sr.MockResponses)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync(sg => sg.ID == id);

            return environment.ToBasicEnvironmentDto();
        }

        public async Task<BaseEnvironmentDto> CreateEnvironment(BaseEnvironmentDto newEnvironmentDto)
        {
            if (newEnvironmentDto == null)
                throw new Exception("No environment provided");

            if (string.IsNullOrWhiteSpace(newEnvironmentDto.Path))
                throw new Exception("Environment path missing or empty");

            if (string.IsNullOrWhiteSpace(newEnvironmentDto.Name))
                throw new Exception("Environment name missing or empty");

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.ID == newEnvironmentDto.TenantId);

            if (tenant == null)
                return null;

            var newEnvironment = new Models.Environment()
            {
                Name = newEnvironmentDto.Name,
                Path = newEnvironmentDto.Path.ToLower(),
                DefaultHealthCheckUrl = newEnvironmentDto.DefaultHealthCheckUrl,
                TenantID = newEnvironmentDto.TenantId,
                Enabled = newEnvironmentDto.Enabled,
                SimulateTime = newEnvironmentDto.SimulateTime
            };

            _context.Environments.Add(newEnvironment);

            await _context.SaveChangesAsync();

            return new BaseEnvironmentDto()
            {
                Id = newEnvironment.ID,
                Name = newEnvironment.Name,
                DefaultHealthCheckUrl = newEnvironment.DefaultHealthCheckUrl,
                Enabled = newEnvironment.Enabled,
                Path = $"{newEnvironment.Path}",
                TenantId = newEnvironment.TenantID,
                SimulateTime = newEnvironment.SimulateTime
            };
        }

        /// <summary>
        /// Update the base properties ONLY on a service
        /// </summary>
        /// <param name="updatedEnvironment">the updated service</param>
        /// <returns>true if updated successfully</returns>
        public async Task<bool> UpdateEnvironmentBaseValues(BaseEnvironmentDto updatedEnvironment)
        {
            var existingEnvironment = await _context.Environments.FirstOrDefaultAsync(sg => sg.ID == updatedEnvironment.Id);

            if (existingEnvironment == null)
                return false;

            if (string.IsNullOrWhiteSpace(updatedEnvironment.Path))
                return false;

            existingEnvironment.Name = updatedEnvironment.Name;
            existingEnvironment.DefaultHealthCheckUrl = updatedEnvironment.DefaultHealthCheckUrl;
            existingEnvironment.Path = updatedEnvironment.Path.ToLower();
            existingEnvironment.Enabled = updatedEnvironment.Enabled;
            existingEnvironment.SimulateTime = updatedEnvironment.SimulateTime;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEnvironment(int id)
        {
            var existingService = await _context.Environments.FirstOrDefaultAsync(sg => sg.ID == id);

            if (existingService == null)
                return false;

            _context.Environments.Remove(existingService);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetEnvironmentId(string path, string environmentPath)
        {
            var tenantPathToLower = path.ToLower();
            var environmentPathToLower = environmentPath.ToLower();

            return (await _context.Environments.FirstOrDefaultAsync(sg => sg.Path == environmentPathToLower && sg.Tenant.Path == tenantPathToLower))?.ID;
        }

        public async Task<int?> GetEnvironmentId(int tenantId, string environmentPath)
        {
            var environmentPathToLower = environmentPath.ToLower();

            return (await _context.Environments.FirstOrDefaultAsync(sg => sg.Path == environmentPathToLower && sg.Tenant.ID == tenantId))?.ID;
        }

        private List<MicroserviceResultDto> GetMicroservicesForEnvironment(int environmentId)
        {
            var microservices = _context.Microservices.Include(ms => ms.Headers)
                .Where(pd => pd.EnvironmentID == environmentId).ToList();

            if (microservices.Count == 0)
                return new List<MicroserviceResultDto>();

            return microservices.Select(ms => new MicroserviceResultDto
            {
                Id = ms.ID,
                Name = ms.Name,
                Enabled = ms.Enabled,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                Path = ms.Path,
                FakeDelay = ms.FakeDelay,
                TargetUrl = ms.TargetUrl,
                RegisteredEnvironmentId = ms.EnvironmentID,
                SimulateTime = ms.SimulateTime,
                HeadersMode = HeadersMode.UserDefined,
                PassThroughTenant = false,
                Headers = ms.Headers?.ToDtos()
            }).ToList();
        }
    }
}
