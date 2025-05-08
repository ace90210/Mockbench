using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Api.Controllers.AdminControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {

        private readonly ILogger<EnvironmentController> _logger;
        private readonly IEnvironmentRepository _environmentRepository;

        public EnvironmentController(ILogger<EnvironmentController> logger, IEnvironmentRepository nvironmentRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environmentRepository = nvironmentRepository ?? throw new ArgumentNullException(nameof(nvironmentRepository));
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BasicEnvironmentDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<IEnumerable<BasicEnvironmentDto>>> Get()
        {
            _logger.LogInformation("Get Environment list called");
            var environments = await _environmentRepository.GetEnvironments();
            
            return Ok(environments);
        }

        [HttpGet("list/{tenantId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EnvironmentOverviewCollection))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<EnvironmentOverviewCollection>> Get(int tenantId)
        {
            if (tenantId <= 0)
                return BadRequest(ErrorMessageConstants.TenantId);
            
            var environments = await _environmentRepository.GetEnvironmentsByTenantId(tenantId);

            if (environments == null)
                return NotFound(ErrorMessageConstants.TenantNotFound);

            return Ok(environments);
        }

        [HttpGet("findbyfullpath/{tenantpath}/{environmentpath}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasicEnvironmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<BasicEnvironmentDto>> GetByFullPath(string tenantpath, string environmentpath)
        {
            if (string.IsNullOrWhiteSpace(tenantpath))
                return BadRequest(ErrorMessageConstants.TenantPath);
            
            if (string.IsNullOrWhiteSpace(environmentpath))
                return BadRequest(ErrorMessageConstants.EnvironmentPath);
            
            var environmentId = await _environmentRepository.GetEnvironmentId(tenantpath, environmentpath);

            if (environmentId == null)
                return NotFound(ErrorMessageConstants.EnvironmentNotFound);

            var environment = await _environmentRepository.GetEnvironmentById(environmentId.Value);

            return Ok(environment);
        }

        [HttpGet("findbypath/{tenant}/{environmentpath}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasicEnvironmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<BasicEnvironmentDto>> GetByFullPath(int tenant, string environmentpath)
        {
            if (tenant <= 0)
                return BadRequest(ErrorMessageConstants.TenantId);
            
            if (string.IsNullOrWhiteSpace(environmentpath))
                return BadRequest(ErrorMessageConstants.EnvironmentPath);
            
            var environmentId = await _environmentRepository.GetEnvironmentId(tenant, environmentpath);

            if (environmentId == null)
                return NotFound(ErrorMessageConstants.EnvironmentNotFound);

            var environment = await _environmentRepository.GetEnvironmentById(environmentId.Value);

            return Ok(environment);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasicEnvironmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<BasicEnvironmentDto>> GetEnvironmentById(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.EnvironmentId);
            
            var environment = await _environmentRepository.GetEnvironmentById(id);

            return environment == null ? NotFound(ErrorMessageConstants.EnvironmentNotFound) : Ok(environment);
        }

        [HttpPost("{tenantId}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseEnvironmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<BaseEnvironmentDto>> CreateEnvironment(int tenantId, [FromBody] BaseEnvironmentDto? newEnvironment)
        {
            if (tenantId <= 0)
                return BadRequest(ErrorMessageConstants.TenantId);
            
            if (newEnvironment == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            if (newEnvironment.Id != 0)
                return BadRequest(ErrorMessageConstants.NewEnvironmentId);

            if (newEnvironment.TenantId > 0 && newEnvironment.TenantId != tenantId)
                return BadRequest(ErrorMessageConstants.IdMissMatch);

            var results = new List<ValidationResult>();

            var existingEnvironmentPaths = await _environmentRepository.GetAllEnvironmentNameAndPathsForTenant(tenantId);

            bool isValid = GeneralHelper.TryValidateFullObject(newEnvironment, new ValidationContext(newEnvironment, 
                new Dictionary<object, object?>()
                {
                    { "Path", existingEnvironmentPaths.Select(sg => sg.Path) },
                    { "Name", existingEnvironmentPaths.Select(sg => sg.Name)}
                }), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            var createdEnvironment = await _environmentRepository.CreateEnvironment(newEnvironment);

            if (createdEnvironment == null)
            {
                return NotFound(ErrorMessageConstants.TenantNotFound);
            }
            
            return StatusCode(201, createdEnvironment);
        }

        [HttpPut("{environmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseEnvironmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<BaseEnvironmentDto>> UpdateEnvironment(int environmentId, [FromBody] BaseEnvironmentDto? updatedEnvironment)
        {
            if (environmentId <= 0)
                return BadRequest(ErrorMessageConstants.EnvironmentId);
            
            if (updatedEnvironment == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            if (updatedEnvironment.Id != environmentId)
                return BadRequest(ErrorMessageConstants.IdMissMatch);

            var results = new List<ValidationResult>();

            var existingEnvironmentPaths = await _environmentRepository.GetAllEnvironmentNameAndPathsForTenant(updatedEnvironment.TenantId, environmentId);

            bool isValid = GeneralHelper.TryValidateFullObject(updatedEnvironment, new ValidationContext(updatedEnvironment, 
                new Dictionary<object, object?>()
                {
                    { "Path", existingEnvironmentPaths.Select(sg => sg.Path) },
                    { "Name", existingEnvironmentPaths.Select(sg => sg.Name)}
                }), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            var updated = await _environmentRepository.UpdateEnvironmentBaseValues(updatedEnvironment);

            if (!updated)
            {
                return NotFound(ErrorMessageConstants.EnvironmentNotFound);
            }

            return Ok(updatedEnvironment);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteEnvironment(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.EnvironmentId);

            if (await _environmentRepository.DeleteEnvironment(id))
                return Ok();

            return NoContent();
        }
    }
}
