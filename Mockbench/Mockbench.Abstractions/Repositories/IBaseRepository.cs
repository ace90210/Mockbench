using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Timetravel;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Abstractions.Repositories
{
    public interface IBaseRepository
    {
        Task<TimeTravelDto> GetMicroserviceTimes(int id);
        Task<TimeTravelDto> GetRequestTimes(int id);
        Task<TimeTravelDto> GetEnvironmentTimes(int id);
        Task<TimeTravelDto> GetTenanEnvironmentTimes(int id);
        Task<bool> SetSimulateTimeOnMicroservice(DateTime? time, int id);
        Task<bool> SetSimulateTimeOnRequest(DateTime? time, int id);
        Task<bool> SetSimulateTimeOnEnvironment(DateTime? time, int id);
        Task<bool> SetSimulateTimeOnTenant(DateTime? time, int id);
        Task<FullDatabaseDto> ExportDatabaseToJson();
        Task<bool> ImportDatabase(FullDatabaseDto import, bool skipDuplicateTenants);
        Task<(bool, MatchingEndpoints?)> CreateTenantEnvironmentMicroserviceIfNotExists(MatchingEndpoints matchingEndpoints);
    }
}