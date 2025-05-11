using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Data.Repositories
{
    public class MockResponseRepository : BaseRepository, IMockResponseRepository
    {
        private readonly MockbenchMainContext _context;

        public MockResponseRepository(MockbenchMainContext context) : base(context)
        {
            _context = context;
        }

        public async Task<MockResponseDto> GetMockResponseAsync(int id)
        {
            var response = await _context.MockResponses.Include(mr => mr.Headers).FirstOrDefaultAsync(rr => rr.ID == id);

            if (response == null)
                return null;

            return response.ToDto(false);
        }

        public async Task<UpdateMockResponseDto> GetUpdateMockResponseAsync(int id)
        {
            var response = await GetMockResponseAsync(id);

            return response?.ToUpdateDto();
        }

        public async Task<UpdateMockResponseDto> PatchMockResponseAsync(int mockResponseId, UpdateMockResponseDto updateMockResponse)
        {
            if (updateMockResponse == null)
                throw new Exception("No response provided");

            var existingMockResponse = await _context.MockResponses.FirstOrDefaultAsync(t => t.ID == mockResponseId);

            if (existingMockResponse == null)
                return null;

            existingMockResponse = existingMockResponse.UpdateWithDto(updateMockResponse, generateChecksum: true);

            _context.MockResponses.Update(existingMockResponse);

            await _context.SaveChangesAsync();

            return existingMockResponse.ToUpdateDto();
        }

        public async Task<MockResponseDto> UpdateMockResponseAsync(int mockResponseId, MockResponseDto updatedResponse)
        {
            if (updatedResponse == null)
                throw new Exception("No response provided");

            if (mockResponseId == 0)
            {
                throw new Exception("No endpoint id provided");
            }

            var endpoint = _context.Endpoints.Include(sr => sr.MockResponses).FirstOrDefault(sr => sr.Id == mockResponseId);

            if (endpoint == null)
                return null;

            if (updatedResponse.Id > 0)
            {
                var existingMockResponse = await _context.MockResponses.FirstOrDefaultAsync(t => t.ID == updatedResponse.Id);

                if (existingMockResponse == null)
                    throw new Exception("Error response does not exist");

                existingMockResponse.Enabled = updatedResponse.Enabled;
                existingMockResponse.Description = updatedResponse.Description;
                existingMockResponse.Body = updatedResponse.Body;
                existingMockResponse.Encoding = updatedResponse.Encoding;
                existingMockResponse.ContentType = updatedResponse.ContentType;
                existingMockResponse.Code = updatedResponse.Code;
                existingMockResponse.Priority = updatedResponse.Priority;
                existingMockResponse.FakeDelay = updatedResponse.FakeDelay;
                existingMockResponse.Checksum = ChecksumHelpers.CreateDefaultChecksum(updatedResponse);
                existingMockResponse.CreatedUtc = updatedResponse.CreatedUtc;

                _context.MockResponses.Update(existingMockResponse);

                await _context.SaveChangesAsync();

                return existingMockResponse.ToDto(false);
            }
            else
            {
                var newModel = updatedResponse.ToEntity(true);
                endpoint.MockResponses.Add(newModel);

                await _context.SaveChangesAsync();

                return newModel.ToDto(false);
            }
        }

        public async Task<(bool success, MockResponseDto? result)> CreateAsync(int endpointId, MockResponseDto response)
        {
            if (response == null)
            {
                throw new Exception("No response provided");
            }

            var existingRequest = _context.Endpoints.Include(r => r.MockResponses).FirstOrDefault(r => r.Id == endpointId);

            if (existingRequest == null)
            {
                return (false, null);
            }
            
            var model = response.ToEntity(true);

            existingRequest.MockResponses.Add(model);
            await _context.SaveChangesAsync();
            return (true, model.ToDto(false));
        }


        public async Task<(bool success, List<MockResponseDto> result)> CreateBulkAsync(int endpointId, List<MockResponseDto> responses)
        {
            if (responses == null)
            {
                throw new Exception("No responses provided");
            }

            if (responses.Count == 0)
            {
                return (true, new List<MockResponseDto>());
            }

            var existingRequest = _context.Endpoints.Include(r => r.MockResponses).FirstOrDefault(r => r.Id == endpointId);

            if (existingRequest == null)
            {
                return (false, null);
            }

            responses.ForEach(action => action.Id = 0);

            var models = responses.ToEntities(true).DistinctBy(nr => nr.Checksum).Where(nr => existingRequest.MockResponses.All(rr => rr.Checksum != nr.Checksum)).ToList();

            if (models.Any())
            {
                existingRequest.MockResponses.AddRange(models);
                await _context.SaveChangesAsync();
                return (true, models.ToDtos(false));
            }
            else
            {
                return (false, new List<MockResponseDto>());
            }
        }


        public async Task<bool> DeleteBulkAsync(int endpointId, List<MockResponseDto> responses)
        {
            if (responses == null || responses.Count == 0)
            {
                return true;
            }

            var existingRequest = _context.Endpoints.Include(r => r.MockResponses).FirstOrDefault(r => r.Id == endpointId);

            if (existingRequest == null)
            {
                return false;
            }

            foreach (var responseToDelete in responses)
            {
                existingRequest.MockResponses.RemoveAll(response => response.ID == responseToDelete.Id);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int responseId)
        {
            if (responseId <= 0)
            {
                return false;
            }

            var existingResponse = _context.MockResponses.FirstOrDefault(r => r.ID == responseId);

            if (existingResponse == null)
            {
                return false;
            }

            _context.MockResponses.Remove(existingResponse);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
