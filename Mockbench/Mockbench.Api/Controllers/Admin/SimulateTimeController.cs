using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.MockServices;
using Mockbench.Shared;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Timetravel;

namespace Mockbench.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulateTimeController : ControllerBase
    {

        private readonly ILogger<SimulateTimeController> _logger;
        private readonly ISimulateTimeService _simulateTimeService;

        public SimulateTimeController(ILogger<SimulateTimeController> logger, ISimulateTimeService simulateTimeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _simulateTimeService = simulateTimeService ?? throw new ArgumentNullException(nameof(simulateTimeService));
        }

        [HttpPost("setsimulate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> SetSimulateAsync(int id, [FromBody] UpdateTimeTravelDto? updateTimeTravelDto)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.ScopeId);

            if (updateTimeTravelDto == null)
            {
                return BadRequest(ErrorMessageConstants.SimulateTimeModelWasNotProvided);
            }
            _logger.LogInformation("Setting simulation time {Time}, {Scope}", updateTimeTravelDto.Time, updateTimeTravelDto.Scope.ToString());
            
            if (await _simulateTimeService.SetSimulateTime(updateTimeTravelDto, id))
            {
                return Ok();
            }

            return BadRequest(ErrorMessageConstants.InvalidTimeScopeType);
        }

        [HttpGet("times/{scope}/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TimeTravelDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetTimesAsync(TimeTravelScope scope, int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.ScopeId);
            
            var endpointDto = await _simulateTimeService.GetTimes(scope, id);

            if (endpointDto == null)
                return BadRequest(ErrorMessageConstants.InvalidTimeScopeType);

            return Ok(endpointDto);
        }
    }
}
