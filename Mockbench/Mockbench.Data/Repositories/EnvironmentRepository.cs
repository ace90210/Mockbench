using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Mappers;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Data.Repositories
{
    public class EnvironmentRepository : BaseRepository, IEnvironmentRepository
    {
        private readonly MockbenchMainContext _context;

        private readonly EnvironmentMapper _environmentMapper = new EnvironmentMapper();

        public EnvironmentRepository(MockbenchMainContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EnvironmentDto>> GetEnvironments()
        {
            var services = await _context.Environments.Include(e => e.Variables).ToListAsync();

            return _environmentMapper.ToEnvironmentDtos(services);
        }

        public async Task<List<PathNameItem>> GetAllEnvironmentNameAndPaths()
        {
            var environmentPaths = _context.Environments.Include(e => e.Variables)
                .Select(sg => new PathNameItem(sg.Name, sg.Path));

            return await environmentPaths.ToListAsync();
        }

        public async Task<List<PathNameItem>> GetAllEnvironmentNameAndPaths(int excludingServiceId)
        {
            var environmentPaths = _context.Environments.Include(e => e.Variables).Where(sg => sg.ID != excludingServiceId)
                .Select(sg => new PathNameItem(sg.Name, sg.Path));

            return await environmentPaths.ToListAsync();
        }

        public async Task<EnvironmentDto?> GetEnvironmentById(int id)
        {
            var environment = await _context.Environments.Include(e => e.Variables)
                                        .FirstOrDefaultAsync(sg => sg.ID == id);

            return environment?.ToBaseEnvironmentDto();
        }

        public async Task<EnvironmentDto> CreateEnvironment(EnvironmentDto newEnvironmentDto)
        {
            if (newEnvironmentDto == null)
                throw new Exception("No environment provided");

            if (string.IsNullOrWhiteSpace(newEnvironmentDto.Path))
                throw new Exception("Environment path missing or empty");

            if (string.IsNullOrWhiteSpace(newEnvironmentDto.Name))
                throw new Exception("Environment name missing or empty");

            var environments = _context.Environments.Include(e => e.Variables);


            var newEnvironment = new Models.Environment()
            {
                Name = newEnvironmentDto.Name,
                Path = newEnvironmentDto.Path.ToLower(),
                DefaultHealthCheckUrl = newEnvironmentDto.DefaultHealthCheckUrl,
                Enabled = newEnvironmentDto.Enabled,
                SimulateTime = newEnvironmentDto.SimulateTime
            };

            _context.Environments.Add(newEnvironment);

            await _context.SaveChangesAsync();

            return new EnvironmentDto()
            {
                Id = newEnvironment.ID,
                Name = newEnvironment.Name,
                DefaultHealthCheckUrl = newEnvironment.DefaultHealthCheckUrl,
                Enabled = newEnvironment.Enabled,
                Path = $"{newEnvironment.Path}",
                SimulateTime = newEnvironment.SimulateTime
            };
        }

        /// <summary>
        /// Update the base properties ONLY on a service
        /// </summary>
        /// <param name="updatedEnvironment">the updated service</param>
        /// <returns>true if updated successfully</returns>
        public async Task<bool> UpdateEnvironmentBaseValues(EnvironmentDto updatedEnvironment)
        {
            var existingEnvironment = await _context.Environments.Include(e => e.Variables).FirstOrDefaultAsync(sg => sg.ID == updatedEnvironment.Id);

            if (existingEnvironment == null)
                return false;

            if (string.IsNullOrWhiteSpace(updatedEnvironment.Path))
                return false;

            existingEnvironment.Name = updatedEnvironment.Name;
            existingEnvironment.DefaultHealthCheckUrl = updatedEnvironment.DefaultHealthCheckUrl;
            existingEnvironment.Path = updatedEnvironment.Path.ToLower();
            existingEnvironment.Enabled = updatedEnvironment.Enabled;
            existingEnvironment.SimulateTime = updatedEnvironment.SimulateTime;

            var toAdd = new List<EnvironmentVariableDto>();
            var toRemove = existingEnvironment.Variables.Where(v => !updatedEnvironment.Variables.Any(uv => uv.Key == v.Key));

            foreach (var variable in toRemove.ToList())
            {
                existingEnvironment.Variables.Remove(variable);
            }

            foreach (var variable in updatedEnvironment.Variables)
            {
                var existingVariable = existingEnvironment.Variables.FirstOrDefault(v => v.Key == variable.Key);
                if (existingVariable != null)
                {
                    existingVariable.Value = variable.Value;
                }
                else
                {
                    toAdd.Add(variable);
                }
            }

            foreach (var variable in toAdd)
            {
                existingEnvironment.Variables.Add(new EnvironmentVariable()
                {
                    Key = variable.Key,
                    Value = variable.Value
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEnvironment(int id)
        {
            var existingService = await _context.Environments.Include(e => e.Variables).FirstOrDefaultAsync(sg => sg.ID == id);

            if (existingService == null)
                return false;

            _context.Environments.Remove(existingService);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetEnvironmentId(string environmentPath)
        {
            var environmentPathToLower = environmentPath.ToLower();

            return (await _context.Environments.Include(e => e.Variables).FirstOrDefaultAsync(sg => sg.Path == environmentPathToLower))?.ID;
        }
    }
}
