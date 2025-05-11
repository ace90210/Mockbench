using Mockbench.Shared;
using Mockbench.Shared.Models.Timetravel;

namespace Mockbench.Abstractions.Services
{
    public interface ISimulateTimeService
    {
        Task<TimeTravelDto> GetTimes(TimeTravelScope scope, int id);
        Task<bool> SetSimulateTime(UpdateTimeTravelDto updateTimeTravel, int id);
    }
}