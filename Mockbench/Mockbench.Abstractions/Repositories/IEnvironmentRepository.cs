using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.General;

namespace Mockbench.Abstractions.Repositories
{
    public interface IEnvironmentRepository
    {
        Task<EnvironmentDto> CreateEnvironment(EnvironmentDto newEnvironmentDto);
        Task<bool> DeleteEnvironment(int id);
        Task<List<PathNameItem>> GetAllEnvironmentNameAndPaths();
        Task<List<PathNameItem>> GetAllEnvironmentNameAndPaths(int excludingEnvironmentId);
        Task<EnvironmentDto> GetEnvironmentById(int id);
        Task<int?> GetEnvironmentId(string environmentPath);
        Task<IEnumerable<EnvironmentDto>> GetEnvironments();
        Task<bool> UpdateEnvironmentBaseValues(EnvironmentDto updatedEnvironment);
    }
}