using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Api.Controllers.AdminControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {

        private readonly ILogger<TenantController> _logger;
        private readonly ITenantRepository _tenantRepository;

        public TenantController(ILogger<TenantController> logger, ITenantRepository tenantRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantNameList))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantNameList>> GetNameList([FromQuery] int skip = 0, [FromQuery] int take = 1000)
        {            
            var tenantList = await _tenantRepository.GetAllTenantsListAsync(skip, take);
            var tenantNameList = new TenantNameList()
            {
                TenantNames = tenantList.Tenants.Select(t => new EntityKeyName() { Id = t.Id, Name = t.Name }).ToList()
            };
            
            return Ok(tenantNameList);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantListDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantListDto>> Get([FromQuery]int skip = 0, [FromQuery]int take = 1000)
        {
            _logger.LogInformation("Get tenant list: skip={Skip}, take={Take}", skip, take);
            return Ok(await _tenantRepository.GetAllTenantsListAsync(skip, take));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantBase))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantBase>> GetTenantById(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.TenantId);
            
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);

            return tenant != null ? Ok(tenant) : NotFound();
        }

        [HttpGet("findbyname/{tenantName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantBase))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantBase>> GetByName(string tenantName)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
                return BadRequest(ErrorMessageConstants.TenantName);
            
            var tenant = await _tenantRepository.GetTenantByNameAsync(tenantName);

            return tenant != null ? Ok(tenant) : NotFound();
        }

        [HttpGet("findbypath/{path}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantBase))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantBase>> GetByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(ErrorMessageConstants.TenantPath);
            
            var tenant = await _tenantRepository.GetTenantByPathAsync(path);

            return tenant != null ? Ok(tenant) : NotFound();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TenantBase))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<TenantBase>> CreateTenant([FromBody] TenantBase newTenant)
        {
            if (newTenant.Id != 0)
                return BadRequest(ErrorMessageConstants.NewTenantId);

            var results = new List<ValidationResult>();

            var existingTenantDetails = await _tenantRepository.GetAllTakenTenantNameAndPathsAsync();
            
            bool isValid = GeneralHelper.TryValidateFullObject(newTenant, new ValidationContext(newTenant, 
                new Dictionary<object, object?>()
                        {
                            { "Path", existingTenantDetails.Select(et => et.Path) },
                            { "Name", existingTenantDetails.Select(et => et.Name) }
                        }), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            return StatusCode(201, await _tenantRepository.CreateTenantAsync(newTenant));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantBase))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpdateTenant(int id, [FromBody]TenantBase? updatedTenant)
        {
            if (id == 0 )
                return BadRequest(ErrorMessageConstants.TenantId);

            if (updatedTenant == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            if (updatedTenant.Id > 0 && updatedTenant.Id != id)
                return BadRequest(ErrorMessageConstants.IdMissMatch);
            
            var results = new List<ValidationResult>();

            updatedTenant.Id = id;
            var existingTenantDetails = await _tenantRepository.GetAllTakenTenantNameAndPathsAsync(updatedTenant.Id);

            bool isValid = GeneralHelper.TryValidateFullObject(updatedTenant, new ValidationContext(updatedTenant, 
                new Dictionary<object, object?>()
                {
                    { "Path", existingTenantDetails.Select(et => et.Path) },
                    { "Name", existingTenantDetails.Select(et => et.Name) }
                }), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            if (await _tenantRepository.UpdateTenantBaseValuesAsync(updatedTenant))
            {
                return Ok(updatedTenant);
            }

            return NotFound(ErrorMessageConstants.TenantNotFound);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteTenant(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.TenantId);
            
            var deleted = await _tenantRepository.DeleteTenantAsync(id);

            if (!deleted)
                return NoContent();

            return Ok();
        }
    }
}
