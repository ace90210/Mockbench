using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class MicroserviceController : ControllerBase
    {

        private readonly ILogger<MicroserviceController> _logger;
        private readonly IMicroserviceRepository _microserviceRepository;

        public MicroserviceController(ILogger<MicroserviceController>? logger, IMicroserviceRepository? microserviceRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _microserviceRepository = microserviceRepository ?? throw new ArgumentNullException(nameof(microserviceRepository));
        }

        [HttpGet("{microserviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroserviceDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MicroserviceDto>> GetById(int microserviceId)
        {
            _logger.LogInformation("Getting microservice: {MicroserviceIdInvalid}", microserviceId);

            if (microserviceId <= 0)
                return BadRequest(ErrorMessageConstants.MicroserviceIdInvalid);
            
            var service =  await _microserviceRepository.GetMicroserviceByIdAsync(microserviceId);

            if (service == null)
                return NotFound(ErrorMessageConstants.MicroserviceNotFound);

            return Ok(service);
        }

        [HttpGet("findbypath/{environmentId}/{microservicePath}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroserviceDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MicroserviceDto>> Get(int environmentId, string? microservicePath)
        {
            if (environmentId <= 0)
                return BadRequest(ErrorMessageConstants.EnvironmentId);
            
            if (string.IsNullOrWhiteSpace(microservicePath))
                return BadRequest(ErrorMessageConstants.MicroservicePath);
            
            var service = await _microserviceRepository.GetMicroservice(microservicePath);

            if(service == null)
                return NotFound(ErrorMessageConstants.MicroserviceNotFound);

            return Ok(service);
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MicroserviceDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<IEnumerable<MicroserviceDto>>> GetAll()
        {
            return Ok(await _microserviceRepository.GetMicroservicesAsync());
        }

        [HttpGet("searchresultlist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MicroserviceDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<IEnumerable<MicroserviceDto>>> GetAllMicroserviceSearchResults()
        {
            return Ok(await _microserviceRepository.GetAllMicroserviceSearchResults());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MicroserviceDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MicroserviceDto>> Create([FromBody] MicroserviceDto? newMicroservice)
        {            
            if (newMicroservice == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            if (newMicroservice.Id != 0)
                return BadRequest(ErrorMessageConstants.NewMicroserviceId);

            if (string.IsNullOrWhiteSpace(newMicroservice.Name))
                return BadRequest(ErrorMessageConstants.MicroserviceName);
            
            if (string.IsNullOrWhiteSpace(newMicroservice.Path))
                return BadRequest(ErrorMessageConstants.MicroservicePath);

            if (newMicroservice.ProxyMode != ProxyMode.None && string.IsNullOrWhiteSpace(newMicroservice.TargetUrl))
                return BadRequest(ErrorMessageConstants.MicroserviceTargetUrl);

            var results = new List<ValidationResult>();

            try
            {
                var existingPaths = await _microserviceRepository.GetAllMicroservicePathAndNames();

                var paths = existingPaths?.Select(ep => ep.Path) ?? Enumerable.Empty<string>();
                var names = existingPaths?.Select(ep => ep.Name) ?? Enumerable.Empty<string>();

                bool isValid = GeneralHelper.TryValidateFullObject(
                    newMicroservice,
                    new ValidationContext(
                        newMicroservice,
                        new Dictionary<object, object?>()
                        {
                            { "Path", paths },
                            { "Name", names }
                        }
                    ),
                    results
                );

                if (!isValid)
                    return BadRequest(results.ToBadRequestResult());

                var createdMicroservice = await _microserviceRepository.CreateMicroserviceAsync(newMicroservice);

                if (createdMicroservice == null)
                    return NotFound(ErrorMessageConstants.EnvironmentNotFound);

                return Created("api/microservice/" + createdMicroservice.Id, createdMicroservice);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{microserviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Update(int microserviceId, [FromBody] MicroserviceDto? updatedMicroservice)
        {
            if (microserviceId <= 0)
                return BadRequest(ErrorMessageConstants.MicroserviceIdInvalid);
            
            if (updatedMicroservice == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            if (updatedMicroservice.Id != 0 && updatedMicroservice.Id != microserviceId)
                return BadRequest(ErrorMessageConstants.IdMissMatch);

            if (string.IsNullOrWhiteSpace(updatedMicroservice.Name))
                return BadRequest(ErrorMessageConstants.MicroserviceName);
            
            if (string.IsNullOrWhiteSpace(updatedMicroservice.Path))
                return BadRequest(ErrorMessageConstants.MicroservicePath);

            if (updatedMicroservice.ProxyMode != ProxyMode.None && string.IsNullOrWhiteSpace(updatedMicroservice.TargetUrl))
                return BadRequest(ErrorMessageConstants.MicroserviceTargetUrl);
            
            var results = new List<ValidationResult>();

            var existingPaths = await _microserviceRepository.GetAllMicroservicePathAndNames(microserviceId);


            var paths = existingPaths?.Select(ep => ep.Path) ?? Enumerable.Empty<string>();
            var names = existingPaths?.Select(ep => ep.Name) ?? Enumerable.Empty<string>();

            bool isValid = GeneralHelper.TryValidateFullObject(
                updatedMicroservice,
                new ValidationContext(
                    updatedMicroservice,
                    new Dictionary<object, object?>()
                    {
                        { "Path", paths },
                        { "Name", names }
                    }
                ),
                results
            );

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            return await _microserviceRepository.UpdateMicroserviceAsync(microserviceId, updatedMicroservice) ? Ok(updatedMicroservice) : NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.MicroserviceIdInvalid);

            if (await _microserviceRepository.DeleteMicroserviceAsync(id))
                return Ok();

            return NoContent();
        }
    }
}
