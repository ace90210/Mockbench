using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Repositories
{
    public class MicroserviceRepository : IMicroserviceRepository
    {
        private readonly MockbenchMainContext _context;

        public MicroserviceRepository(MockbenchMainContext context)
        {
            _context = context;
        }

        public async Task<List<PathNameItem>> GetAllMicroservicePathAndNamesForEnvironment(int environmentId)
        {
            var paths = _context.Microservices.Where(pd => pd.EnvironmentID == environmentId)
                                                                                    .Select(rs => new PathNameItem(rs.Name, rs.Path));

            return await paths.ToListAsync();
        }

        public async Task<List<PathNameItem>> GetAllMicroservicePathAndNamesForEnvironment(int environmentId, int excludingMicroserviceId)
        {
            var paths = _context.Microservices.Where(pd => pd.EnvironmentID == environmentId && pd.ID != excludingMicroserviceId).Select(rs => new PathNameItem(rs.Name, rs.Path));

            return await paths.ToListAsync();
        }

        public async Task<MicroserviceResultDto> GetMicroservice(int environmentId, string microservicePath)
        {
            var ms = await _context.Microservices.FirstOrDefaultAsync(ms => ms.EnvironmentID == environmentId && ms.Path == microservicePath);

            return new MicroserviceResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos()
            };
        }

        public async Task<MicroserviceResultDto> GetMicroserviceById(int id)
        {
            var ms = await _context.Microservices.Include(m => m.Headers).FirstOrDefaultAsync(ms => ms.ID == id);

            if (ms == null)
                return null;

            return new MicroserviceResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos()
            };
        }

        public async Task<IEnumerable<MicroserviceResultDto>> GetAllMicroserviceForTenant(int tenantId)
        {
            var microservices = await _context.Microservices.Include(p => p.Environment).Where(pd => pd.Environment != null && pd.Environment.TenantID == tenantId).ToListAsync();

            return microservices.Select(ms => new MicroserviceResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos()
            });
        }

        public async Task<IEnumerable<MicroserviceResultDto>> GetAllMicroservices()
        {
            var microservices = await _context.Microservices.ToListAsync();

            return microservices.Select(ms => new MicroserviceResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos()
            });
        }

        public async Task<IEnumerable<MicroserviceSearchResultDto>> GetAllMicroserviceSearchResults()
        {
            var microservices = await _context.Microservices
                                                            .Include(m => m.Endpoints)
                                                            .Include(m => m.Environment)
                                                            .ThenInclude(sg => sg.Tenant)
                                                            .AsSplitQuery()
                                                            .ToListAsync();

            return microservices.Select(ms => new MicroserviceSearchResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos(),
                TenantId = ms.Environment.TenantID,
                TenantName = ms.Environment.Tenant.Name,
                EnvironmentId = ms.EnvironmentID,
                EnvironmentName = ms.Environment.Name,
                TotalEndpoints = ms.Endpoints?.Count ?? 0
            });
            
        }

        public async Task<IEnumerable<MicroserviceResultDto>> GetAllMicroservicesForEnvironment(int environmentId)
        {
            var microservices = await _context.Microservices.Where(pd => pd.EnvironmentID == environmentId).ToListAsync();

            return microservices.Select(ms => new MicroserviceResultDto()
            {
                Id = ms.ID,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                RegisteredEnvironmentId = ms.EnvironmentID,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos()
            });
        }

        public async Task<MicroserviceResultDto> FindMicroservice(string tenantPath, string environmentPath, string path)
        {
            if (string.IsNullOrWhiteSpace(tenantPath) || string.IsNullOrWhiteSpace(environmentPath) || string.IsNullOrWhiteSpace(path))
                return null;

            var microservice = await _context.Microservices
                .Include(m => m.Headers)
                .Include(m => m.Environment)
                .ThenInclude(rs => rs.Tenant)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m =>
                        m.Path.ToLower() == path.ToLower() && 
                        m.Environment.Path.ToLower() == environmentPath.ToLower() &&
                        m.Environment.Tenant.Path.ToLower() == tenantPath.ToLower()
                    );

            if (microservice == null) return null;

            return new MicroserviceResultDto()
            {
                Id = microservice.ID,
                Name = microservice.Name,
                Path = microservice.Path,
                Enabled = microservice.Enabled,
                PassThroughTenant = microservice.PassThroughTenant,
                FakeDelay = microservice.FakeDelay,
                RegisteredEnvironmentId = microservice.EnvironmentID,
                TargetUrl = microservice.TargetUrl,
                ProxyMode = microservice.ProxyMode,
                RandomiseMockResult = microservice.RandomiseMockResult,
                HeadersMode = microservice.HeadersMode,
                InjectForwardingHeadersOnRequest = microservice.InjectForwardingHeadersOnRequest,
                Headers = microservice.Headers.ToDtos(),
                SimulateTime = microservice.SimulateTime
            };
        }

        public async Task<MatchingEndpointMicroserviceDetailsDto> FindMatchingRequest(string tenantPath, string environmentPath, string path)
        {
            if (string.IsNullOrWhiteSpace(tenantPath) || string.IsNullOrWhiteSpace(environmentPath) || string.IsNullOrWhiteSpace(path))
                return null;

            var microservice = await _context.Microservices.Include(m => m.Headers)
                .Include(m => m.Environment)
                .ThenInclude(rs => rs.Tenant)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m =>
                    m.Path.ToLower() == path.ToLower() && 
                    m.Environment.Path.ToLower() == environmentPath.ToLower() &&
                    m.Environment.Tenant.Path.ToLower() == tenantPath.ToLower()
                );

            if (microservice == null) return null;

            return new MatchingEndpointMicroserviceDetailsDto()
            {
                Microservice = new MicroserviceResultDto()
                {
                    Id = microservice.ID,
                    Name = microservice.Name,
                    Path = microservice.Path,
                    Enabled = microservice.Enabled,
                    PassThroughTenant = microservice.PassThroughTenant,
                    FakeDelay = microservice.FakeDelay,
                    RegisteredEnvironmentId = microservice.EnvironmentID,
                    TargetUrl = microservice.TargetUrl,
                    ProxyMode = microservice.ProxyMode,
                    RandomiseMockResult = microservice.RandomiseMockResult,
                    Headers = microservice.Headers.ToDtos(),
                    HeadersMode = microservice.HeadersMode,
                    InjectForwardingHeadersOnRequest = microservice.InjectForwardingHeadersOnRequest,
                    SimulateTime = microservice.SimulateTime
                },
                Environment = microservice.Environment.ToBasicEnvironmentDto(),
                Tenant = new BaseTenantDto() {
                    Id = microservice.Environment.Tenant.ID,
                    Name = microservice.Environment.Tenant.Name,
                    SimulateTime = microservice.Environment.Tenant.SimulateTime,
                    Path = microservice.Environment.Tenant.Path
                }
            };
        }

        public async Task<MicroserviceResultDto> CreateMicroservice(MicroserviceResultDto newMicroserviceDto)
        {
            var service = await _context.Environments.FirstOrDefaultAsync(t => t.ID == newMicroserviceDto.RegisteredEnvironmentId);

            if (service == null)
                return null;

            if (newMicroserviceDto == null)
                throw new Exception("Error new microservice not provided");

            if (string.IsNullOrWhiteSpace(newMicroserviceDto.Path))
                throw new Exception("Error microservice must have a path specified");

            var newMicroservice = new Microservice()
            {
                Name = newMicroserviceDto.Name,
                TargetUrl = newMicroserviceDto.TargetUrl,
                Path = newMicroserviceDto.Path.ToLower(),
                EnvironmentID = newMicroserviceDto.RegisteredEnvironmentId,
                Enabled = newMicroserviceDto.Enabled,
                PassThroughTenant = newMicroserviceDto.PassThroughTenant,
                FakeDelay = newMicroserviceDto.FakeDelay,
                ProxyMode = newMicroserviceDto.ProxyMode,
                RandomiseMockResult = newMicroserviceDto.RandomiseMockResult,
                Headers = newMicroserviceDto.Headers.ToModels(),
                HeadersMode = newMicroserviceDto.HeadersMode,
                InjectForwardingHeadersOnRequest = newMicroserviceDto.InjectForwardingHeadersOnRequest,
                SimulateTime = newMicroserviceDto.SimulateTime
            };

            _context.Microservices.Add(newMicroservice);

            await _context.SaveChangesAsync();

            return new MicroserviceResultDto()
            {
                Id = newMicroservice.ID,
                Name = newMicroservice.Name,
                TargetUrl = newMicroservice.TargetUrl,
                Path = newMicroservice.Path,
                RegisteredEnvironmentId = newMicroservice.EnvironmentID,
                Enabled = newMicroservice.Enabled,
                PassThroughTenant = newMicroservice.PassThroughTenant,
                FakeDelay = newMicroservice.FakeDelay,
                ProxyMode = newMicroservice.ProxyMode,
                RandomiseMockResult = newMicroservice.RandomiseMockResult,
                Headers = newMicroservice.Headers.ToDtos(),                
                HeadersMode = newMicroserviceDto.HeadersMode,
                InjectForwardingHeadersOnRequest = newMicroserviceDto.InjectForwardingHeadersOnRequest,
                SimulateTime = newMicroserviceDto.SimulateTime
            };
        }

        public async Task<bool> UpdateMicroservice(MicroserviceResultDto updatedMicroservice)
        {
            if (updatedMicroservice == null)
                return false;

            if (string.IsNullOrWhiteSpace(updatedMicroservice.Path))
                return false;

            var existingMicroservice = await _context.Microservices.Include(m => m.Headers).FirstOrDefaultAsync(t => t.ID == updatedMicroservice.Id);

            if (existingMicroservice == null)
                return false;

            existingMicroservice.Name = updatedMicroservice.Name;
            existingMicroservice.Path = updatedMicroservice.Path.ToLower();
            existingMicroservice.TargetUrl = updatedMicroservice.TargetUrl;
            existingMicroservice.Enabled = updatedMicroservice.Enabled;
            existingMicroservice.PassThroughTenant = updatedMicroservice.PassThroughTenant;
            existingMicroservice.FakeDelay = updatedMicroservice.FakeDelay;
            existingMicroservice.ProxyMode = updatedMicroservice.ProxyMode;
            existingMicroservice.RandomiseMockResult = updatedMicroservice.RandomiseMockResult;
            existingMicroservice.SimulateTime = updatedMicroservice.SimulateTime;
            existingMicroservice.HeadersMode = updatedMicroservice.HeadersMode;
            existingMicroservice.InjectForwardingHeadersOnRequest = updatedMicroservice.InjectForwardingHeadersOnRequest;

            if (updatedMicroservice.Headers == null || updatedMicroservice.Headers.Count == 0)
            {
                //  clearing all headers
                existingMicroservice.Headers?.Clear();
            }
            else
            {
                // updating headers
                if (existingMicroservice.Headers == null || existingMicroservice.Headers.Count == 0)
                {
                    // Adding headers to empty list
                    existingMicroservice.Headers = updatedMicroservice.Headers.ToModels();
                }
                else
                {
                    // merging headers

                    // delete headers not in updated list
                    var headersToDelete = existingMicroservice.Headers?
                                        .Where(eh => !updatedMicroservice.Headers?.Any(uh => uh.Name == eh.Name) ?? false)
                                        .Select(eh => eh.ID).ToList();

                    if (headersToDelete != null && headersToDelete.Any())
                    {
                        existingMicroservice.Headers.RemoveAll(eh => headersToDelete.Any(htd => htd == eh.ID));
                    }

                    // add headers missing from current list
                    var headersToAdd = updatedMicroservice.Headers?
                                            .Where(uh => existingMicroservice.Headers.All(eh => eh.Name != uh.Name))
                                            .Select(h => h.ToEntity())
                                            .ToList();

                    if (headersToAdd != null && headersToAdd.Any())
                    {
                        if (existingMicroservice.Headers == null)
                        {
                            existingMicroservice.Headers = headersToAdd;
                        }
                        else
                        {
                            existingMicroservice.Headers.AddRange(headersToAdd);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMicroservice(int id)
        {
            var existingMicroservice = await _context.Microservices.FirstOrDefaultAsync(m => m.ID == id);

            if (existingMicroservice == null)
                return false;

            _context.Microservices.Remove(existingMicroservice);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MicroserviceParentIds> GetParentIds(int microserviceId)
        {
            var microservice = await _context.Microservices.Include(m => m.Environment)
                .ThenInclude(sg => sg.Tenant)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.ID == microserviceId);

            if(microservice == null)
                return null;

            return new MicroserviceParentIds() 
            {
                MicroserviceId = microservice.ID,
                EnvironmentId = microservice.Environment.ID,
                TenantId = microservice.Environment.TenantID
            };
        }
    }
}
