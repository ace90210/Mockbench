using Mockbench.Abstractions.Services;
using Mockbench.Abstractions.Repositories;
using Mockbench.Shared;
using Mockbench.Shared.Models.Timetravel;

namespace Mockbench.Services.MockServices
{
    public class SimulateTimeService : ISimulateTimeService
    {
        private readonly IBaseRepository _baseRepository;

        public SimulateTimeService(IBaseRepository baseRepository)
        {
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
        }

        public async Task<TimeTravelDto> GetTimes(TimeTravelScope scope, int id)
        {
            switch (scope)
            {
                case TimeTravelScope.Endpoint:       return await _baseRepository.GetRequestTimes(id);
                case TimeTravelScope.Microservice: return await _baseRepository.GetMicroserviceTimes(id);
                case TimeTravelScope.Environment: return await _baseRepository.GetEnvironmentTimes(id);
                case TimeTravelScope.Tenant: return await _baseRepository.GetTenanEnvironmentTimes(id);
                default: return null; 
            }
        }

        public async Task<bool> SetSimulateTime(UpdateTimeTravelDto updateTimeTravel, int id)
        {
            switch (updateTimeTravel.Scope)
            {
                case TimeTravelScope.Endpoint: return await _baseRepository.SetSimulateTimeOnRequest(updateTimeTravel.Time, id);
                case TimeTravelScope.Microservice: return await _baseRepository.SetSimulateTimeOnMicroservice(updateTimeTravel.Time, id);
                case TimeTravelScope.Environment: return await _baseRepository.SetSimulateTimeOnEnvironment(updateTimeTravel.Time, id);
                case TimeTravelScope.Tenant: return await _baseRepository.SetSimulateTimeOnTenant(updateTimeTravel.Time, id);
                default: return false;
            }
        }
    }
}
