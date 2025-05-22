using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Response;
using Mockbench.Data.Mappers;

namespace Mockbench.Data.Repositories
{
    public class EndpointRepository : BaseRepository, IEndpointRepository
    {
        private readonly MockbenchDbContext _context;
        public EndpointRepository(MockbenchDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<EndpointDto> GetEndpoint(int id)
        {
            var e = await _context.Endpoints.Include(e => e.MockResponses)
                                                    .Include(e => e.QueryParameters)
                                                    .Include(e => e.EndpointHeaders)
                                                    .AsSplitQuery()
                                                    .FirstOrDefaultAsync(rr => rr.Id == id);

            if (e == null)
                return null;

            return e.ToDto(createNew: false);
        }

        public async Task<UpdateEndpointDto> GetUpdateEndpoint(int id)
        {
            var e = await GetEndpoint(id);

            return e?.ToUpdateDto();
        }

        public async Task<MatchingEndpoints> GetAllMatchingEndpointsAsync(string? tenantPath, string? environmentPath, string? microservicePath, string endpointUrl)
        {
            var endpoint = await _context.Endpoints
                                             .Include(e => e.QueryParameters)
                                             .Include(e => e.EndpointHeaders)
                                             .Include(e => e.MockResponses)
                                                 .ThenInclude(mr => mr.Headers)
                                             .Where(e =>
                                                                 ((e.Tenant == null || e.Tenant.Path == null) && tenantPath == null) ||
                                                                 (e.Tenant != null && e.Tenant.Path == tenantPath)
                                                            &&
                                                                 ((e.Environment == null || e.Environment.Path == null) && environmentPath == null) ||
                                                                 (e.Environment != null && e.Environment.Path == environmentPath)
                                                            &&
                                                                 ((e.Microservice == null || e.Microservice.Path == null) && microservicePath == null) ||
                                                                 (e.Microservice != null && e.Microservice.Path == microservicePath)
                                                            && (endpointUrl.StartsWith(e.FromUrl))
                                                             )
                                                             .AsSplitQuery()
                                                             .ToListAsync();

            var tenant = _context.Tenants.Include(t => t.Variables).FirstOrDefault(t => t.Path == tenantPath);
            var environment = _context.Environments.Include(t => t.Variables).FirstOrDefault(e => e.Path == environmentPath);
            var microservice = _context.Microservices.FirstOrDefault(m => m.Path == microservicePath);

            return new MatchingEndpoints() {
                Tenant = Mapper.Tenant.ToDto(tenant),
                Environment = Mapper.Environment.ToDto(environment),
                Microservice = Mapper.Microservice.ToDto(microservice),
                TenantPath = tenantPath,
                EnvironmentPath = environmentPath,
                MicroservicePath = microservicePath,
                Endpoints = endpoint.ToDtos(createNew: false)
            };
        }

        public async Task<List<EndpointDto>> GetAllEndpointsForMicroserviceAsync(int microserviceId)
        {
            var endpoints = await _context.Endpoints.Include(e => e.QueryParameters)
                .Include(e => e.EndpointHeaders)
                .Include(e => e.MockResponses)
                    .ThenInclude(mr => mr.Headers)
                .Where(m => m.MicroserviceId == microserviceId)
                .AsSplitQuery()
                .ToListAsync();

            return endpoints.ToDtos(false);
        }

        public async Task<EndpointDto> CreateEndpointAsync(EndpointDto endpointDto)
        {
            if (endpointDto == null)
                throw new Exception("No endpoint provided");
            
            var endpoint = endpointDto.ToEntity(createNew: true);

            _context.Endpoints.Add(endpoint);

            await _context.SaveChangesAsync();

            return endpoint.ToDto(createNew: false);
        }

        public async Task<UpdateEndpointDto> UpdateEndpoint(int endpointId, UpdateEndpointDto endpointDto)
        {
            if (endpointDto == null)
                throw new Exception("No endpoint provided");

            var existingendpoint = await _context.Endpoints.Include(e => e.MockResponses)
                                                                        .Include(e => e.QueryParameters)
                                                                        .Include(e => e.EndpointHeaders)
                                                                        .AsSplitQuery()
                                                                        .FirstOrDefaultAsync(t => t.Id == endpointId);

            if (existingendpoint == null)
                return null;

            existingendpoint = existingendpoint.UpdateWithDto(endpointDto);

            _context.Endpoints.Update(existingendpoint);

            await _context.SaveChangesAsync();

            return existingendpoint.ToUpdateDto();
        }

        public async Task<EndpointDto> UpdateMockResponses(int endpointId, List<MockResponseDto> responses)
        {
            if (endpointId <= 0)
                throw new Exception("Invalid response id: " + endpointId);

            if (responses == null)
                throw new Exception("No responses provided");

            var existingendpoint = await _context.Endpoints.Include(e => e.MockResponses)
                                                                        .Include(e => e.QueryParameters)
                                                                        .Include(e => e.EndpointHeaders)
                                                                        .AsSplitQuery()
                                                                        .FirstOrDefaultAsync(t => t.Id == endpointId);

            if (existingendpoint == null)
                return null;

            existingendpoint = existingendpoint.MergeResponses(responses);

            _context.Endpoints.Update(existingendpoint);

            await _context.SaveChangesAsync();

            return existingendpoint.ToDto(false);
        }

        public async Task<bool> DeleteEndpoint(int endpointId)
        {
            var existedRequest = await _context.Endpoints.FirstOrDefaultAsync(rd => rd.Id == endpointId);

            if (existedRequest == null)
                return false;

            _context.Endpoints.Remove(existedRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddResponseToEndpointAsync(int endpointId, MockResponseDto mockResponseDto)
        {
            mockResponseDto.EndpointId = endpointId;

            var newMockResponse = mockResponseDto.ToEntity();

            _context.MockResponses.Add(newMockResponse);

            await _context.SaveChangesAsync();
        }

        public async Task<DateTime[]> GetMockResponseTimes(int endpointId)
        {
            var dateTimes = _context.MockResponses.Where(rr => rr.EndpointId == endpointId).Select(rr => rr.CreatedUtc!.Value);

            return await dateTimes.ToArrayAsync();
        }
    }
}
