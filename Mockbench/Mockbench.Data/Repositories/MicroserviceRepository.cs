using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Data.Repositories
{
    public class MicroserviceRepository : BaseRepository, IMicroserviceRepository
    {
        private readonly MockbenchDbContext _context;

        public MicroserviceRepository(MockbenchDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PathNameItem>> GetAllMicroservicePathAndNames()
        {
            var paths = _context.Microservices.Select(rs => new PathNameItem(rs.Name, rs.Path));

            return await paths.ToListAsync();
        }

        public async Task<List<PathNameItem>> GetAllMicroservicePathAndNames(int excludingMicroserviceId)
        {
            var paths = _context.Microservices.Where(pd => pd.Id != excludingMicroserviceId).Select(rs => new PathNameItem(rs.Name, rs.Path));

            return await paths.ToListAsync();
        }

        public async Task<MicroserviceDto> GetMicroservice(string microservicePath)
        {
            var ms = await _context.Microservices.Include(ms => ms.Headers).Include(ms => ms.Endpoints).AsSplitQuery().FirstOrDefaultAsync(ms => ms.Path == microservicePath);

            return new MicroserviceDto()
            {
                Id = ms.Id,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos(),
                Endpoints = ms.Endpoints.ToDtos(false)
            };
        }

        public async Task<MicroserviceDto> GetMicroserviceByIdAsync(int id)
        {
            var ms = await _context.Microservices.Include(m => m.Headers).Include(ms => ms.Endpoints).AsSplitQuery().FirstOrDefaultAsync(ms => ms.Id == id);

            if (ms == null)
                return null;

            return new MicroserviceDto()
            {
                Id = ms.Id,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos(),
                Endpoints = ms.Endpoints.ToDtos(false)
            };
        }

        public async Task<IEnumerable<MicroserviceDto>> GetMicroservicesAsync()
        {
            var microservices = await _context.Microservices.Include(m => m.Headers).Include(ms => ms.Endpoints).AsSplitQuery().ToListAsync();

            return microservices.Select(ms => new MicroserviceDto()
            {
                Id = ms.Id,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos(),
                Endpoints = ms.Endpoints.ToDtos(false)
            });
        }

        public async Task<IEnumerable<MicroserviceDto>> GetAllMicroserviceSearchResults()
        {
            var microservices = await _context.Microservices
                                                            .Include(m => m.Headers)
                                                            .Include(m => m.Endpoints)
                                                            .AsSplitQuery()
                                                            .ToListAsync();

            return microservices.Select(ms => new MicroserviceDto()
            {
                Id = ms.Id,
                Name = ms.Name,
                Path = ms.Path,
                Enabled = ms.Enabled,
                PassThroughTenant = ms.PassThroughTenant,
                FakeDelay = ms.FakeDelay,
                TargetUrl = ms.TargetUrl,
                ProxyMode = ms.ProxyMode,
                RandomiseMockResult = ms.RandomiseMockResult,
                HeadersMode = ms.HeadersMode,
                InjectForwardingHeadersOnRequest = ms.InjectForwardingHeadersOnRequest,
                SimulateTime = ms.SimulateTime,
                Headers = ms.Headers.ToDtos(),
                Endpoints = ms.Endpoints.ToDtos(false)
            });
            
        }

        public async Task<MicroserviceDto> FindMicroservice(string tenantPath, string environmentPath, string path)
        {
            if (string.IsNullOrWhiteSpace(tenantPath) || string.IsNullOrWhiteSpace(environmentPath) || string.IsNullOrWhiteSpace(path))
                return null;

            var microservice = await _context.Microservices
                .Include(m => m.Headers)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m =>
                        m.Path.ToLower() == path.ToLower() // TODO implement this method properly
                    );

            if (microservice == null) return null;

            return new MicroserviceDto()
            {
                Id = microservice.Id,
                Name = microservice.Name,
                Path = microservice.Path,
                Enabled = microservice.Enabled,
                PassThroughTenant = microservice.PassThroughTenant,
                FakeDelay = microservice.FakeDelay,
                TargetUrl = microservice.TargetUrl,
                ProxyMode = microservice.ProxyMode,
                RandomiseMockResult = microservice.RandomiseMockResult,
                HeadersMode = microservice.HeadersMode,
                InjectForwardingHeadersOnRequest = microservice.InjectForwardingHeadersOnRequest,
                Headers = microservice.Headers?.ToDtos(),
                SimulateTime = microservice.SimulateTime,
                Endpoints = microservice.Endpoints?.ToDtos(false)
            };
        }

        public async Task<MicroserviceDto> CreateMicroserviceAsync(MicroserviceDto newMicroserviceDto)
        {
            if (newMicroserviceDto == null)
                throw new Exception("Error new microservice not provided");

            if (string.IsNullOrWhiteSpace(newMicroserviceDto.Path))
                throw new Exception("Error microservice must have a path specified");

            var newMicroservice = new Microservice()
            {
                Name = newMicroserviceDto.Name,
                TargetUrl = newMicroserviceDto.TargetUrl,
                Path = newMicroserviceDto.Path.ToLower(),
                Enabled = newMicroserviceDto.Enabled,
                PassThroughTenant = newMicroserviceDto.PassThroughTenant,
                FakeDelay = newMicroserviceDto.FakeDelay,
                ProxyMode = newMicroserviceDto.ProxyMode,
                RandomiseMockResult = newMicroserviceDto.RandomiseMockResult,
                Headers = newMicroserviceDto.Headers?.ToEntities(),
                HeadersMode = newMicroserviceDto.HeadersMode,
                InjectForwardingHeadersOnRequest = newMicroserviceDto.InjectForwardingHeadersOnRequest,
                SimulateTime = newMicroserviceDto.SimulateTime,
                Endpoints = newMicroserviceDto.Endpoints?.ToEntities(false)
            };

            _context.Microservices.Add(newMicroservice);

            await _context.SaveChangesAsync();

            return new MicroserviceDto()
            {
                Id = newMicroservice.Id,
                Name = newMicroservice.Name,
                TargetUrl = newMicroservice.TargetUrl,
                Path = newMicroservice.Path,
                Enabled = newMicroservice.Enabled,
                PassThroughTenant = newMicroservice.PassThroughTenant,
                FakeDelay = newMicroservice.FakeDelay,
                ProxyMode = newMicroservice.ProxyMode,
                RandomiseMockResult = newMicroservice.RandomiseMockResult,
                Headers = newMicroservice.Headers?.ToDtos(),                
                HeadersMode = newMicroserviceDto.HeadersMode,
                InjectForwardingHeadersOnRequest = newMicroserviceDto.InjectForwardingHeadersOnRequest,
                SimulateTime = newMicroserviceDto.SimulateTime,
                Endpoints = newMicroserviceDto.Endpoints
            };
        }

        public async Task<bool> UpdateMicroserviceAsync(int id, MicroserviceDto updatedMicroservice)
        {
            if (updatedMicroservice == null)
                return false;

            if (string.IsNullOrWhiteSpace(updatedMicroservice.Path))
                return false;

            var existingMicroservice = await _context.Microservices.Include(m => m.Headers).FirstOrDefaultAsync(t => t.Id == id);

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
                    existingMicroservice.Headers = updatedMicroservice.Headers.ToEntities();
                }
                else
                {
                    // merging headers

                    // delete headers not in updated list
                    var headersToDelete = existingMicroservice.Headers?
                                        .Where(eh => !updatedMicroservice.Headers?.Any(uh => uh.Name == eh.Name) ?? false)
                                        .Select(eh => eh.Id).ToList();

                    if (headersToDelete is not null && headersToDelete.Any())
                    {
                        existingMicroservice.Headers.RemoveAll(eh => headersToDelete.Any(htd => htd == eh.Id ));
                    }

                    // add headers missing from current list
                    var headersToAdd = updatedMicroservice.Headers?
                                            .Where(uh => existingMicroservice.Headers.All(eh => eh.Name != uh.Name))
                                            .Select(h => h.ToEntity())
                                            .ToList();

                    if (headersToAdd is not null && headersToAdd.Any())
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

        public async Task<bool> DeleteMicroserviceAsync(int id)
        {
            var existingMicroservice = await _context.Microservices.FirstOrDefaultAsync(m => m.Id == id);

            if (existingMicroservice == null)
                return false;

            _context.Microservices.Remove(existingMicroservice);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
