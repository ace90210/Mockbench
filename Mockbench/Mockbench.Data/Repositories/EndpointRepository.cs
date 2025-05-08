using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Data.Repositories
{
    public class EndpointRepository : IEndpointRepository
    {
        private readonly MockbenchMainContext _context;

        public EndpointRepository(MockbenchMainContext context)
        {
            _context = context;
        }

        public async Task<EndpointDto> GetEndpoint(int id)
        {
            var sr = await _context.Endpoints.Include(sr => sr.MockResponses)
                                                    .Include(sr => sr.QueryParameters)
                                                    .Include(sr => sr.EndpointHeaders)
                                                    .AsSplitQuery()
                                                    .FirstOrDefaultAsync(rr => rr.ID == id);

            if (sr == null)
                return null;

            return sr.ToDto(createNew: false, createChecksumOnResponses: true);
        }

        public async Task<UpdateEndpointDto> GetUpdateEndpoint(int id)
        {
            var sr = await GetEndpoint(id);

            return sr?.ToUpdateDto();
        }

        public async Task<IEnumerable<EndpointDto>> GetAllEndpointsForMicroserviceAsync(int microserviceId)
        {
            var endpoint = await _context.Endpoints.Include(sr => sr.QueryParameters)
                                                                .Include(sr => sr.EndpointHeaders)
                                                                .Include(sr => sr.MockResponses)
                                                                .ThenInclude(mr => mr.Headers)
                                                                .Where(sr => sr.MicroserviceID == microserviceId)
                                                                .AsSplitQuery()
                                                                .ToListAsync();

            return endpoint.ToDtos(createNew: false, createChecksumOnResponses: false);
        }

        public async Task<EndpointDto> CreateEndpointAsync(int microserviceId, EndpointDto endpointDto)
        {
            if (endpointDto == null)
                throw new Exception("No endpoint provided");

            var microserviceExists = _context.Microservices.Any(m => m.ID == microserviceId);

            if (!microserviceExists)
                return null;
            
            endpointDto.MicroserviceId = microserviceId;

            var endpoint = endpointDto.ToEntity(createNew: true, createChecksumOnResponses: true);

            _context.Endpoints.Add(endpoint);

            await _context.SaveChangesAsync();

            return endpoint.ToDto(createNew: false, createChecksumOnResponses: true);
        }

        public async Task<UpdateEndpointDto> UpdateEndpoint(int endpointId, UpdateEndpointDto endpointDto)
        {
            if (endpointDto == null)
                throw new Exception("No endpoint provided");

            var existingendpoint = await _context.Endpoints.Include(sr => sr.MockResponses)
                                                                        .Include(sr => sr.QueryParameters)
                                                                        .Include(sr => sr.EndpointHeaders)
                                                                        .AsSplitQuery()
                                                                        .FirstOrDefaultAsync(t => t.ID == endpointId);

            if (existingendpoint == null)
                return null;

            existingendpoint = existingendpoint.UpdateWithDto(endpointDto);

            _context.Endpoints.Update(existingendpoint);

            await _context.SaveChangesAsync();

            return existingendpoint.ToUpdateDto(false);
        }

        public async Task<EndpointDto> UpdateMockResponses(int endpointId, List<MockResponseDto> responses)
        {
            if (endpointId <= 0)
                throw new Exception("Invalid response id: " + endpointId);

            if (responses == null)
                throw new Exception("No responses provided");

            var existingendpoint = await _context.Endpoints.Include(sr => sr.MockResponses)
                                                                        .Include(sr => sr.QueryParameters)
                                                                        .Include(sr => sr.EndpointHeaders)
                                                                        .AsSplitQuery()
                                                                        .FirstOrDefaultAsync(t => t.ID == endpointId);

            if (existingendpoint == null)
                return null;

            existingendpoint = existingendpoint.MergeResponses(responses);

            _context.Endpoints.Update(existingendpoint);

            await _context.SaveChangesAsync();

            return existingendpoint.ToDto(false, false);
        }

        public async Task<bool> DeleteEndpoint(int endpointId)
        {
            var existedRequest = await _context.Endpoints.FirstOrDefaultAsync(rd => rd.ID == endpointId);

            if (existedRequest == null)
                return false;

            _context.Endpoints.Remove(existedRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddResponseToEndpointAsync(int endpointId, MockResponseDto mockResponseDto)
        {
            mockResponseDto.EndpointId = endpointId;

            var newMockResponse = mockResponseDto.ToEntity(true);

            _context.MockResponses.Add(newMockResponse);

            await _context.SaveChangesAsync();
        }

        public async Task<DateTime[]> GetMockResponseTimes(int endpointId)
        {
            var dateTimes = _context.MockResponses.Where(rr => rr.EndpointId == endpointId).Select(rr => rr.CreatedUtc);

            return await dateTimes.ToArrayAsync();
        }
    }
}
