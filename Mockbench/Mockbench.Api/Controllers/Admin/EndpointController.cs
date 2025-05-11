using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Helpers;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;
using Mockbench.Shared.Models.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController : ControllerBase
    {
        private readonly ILogger<EndpointController> _logger;
        private readonly IEndpointRepository _endpointRepository;

        public EndpointController(ILogger<EndpointController> logger, IEndpointRepository endpointRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _endpointRepository = endpointRepository ?? throw new ArgumentNullException(nameof(endpointRepository));
        }

        // TODO reimplement this
        //[HttpGet("list/{microserviceId}")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EndpointDto>))]
        //[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        //public async Task<ActionResult<IEnumerable<EndpointDto>>> GetAllForMicroservice(int microserviceId)
        //{
        //    if (microserviceId <= 0)
        //        return BadRequest(ErrorMessageConstants.MicroserviceId);

        //    _logger.LogInformation("get request for microservice: {MicroserviceId}", microserviceId);
        //    return Ok(await _endpointRepository.GetAllMatchingEndpointsAsync(microserviceId));
        //}

        [HttpGet("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EndpointDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<EndpointDto>> GetById(int endpointId)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);

            var endpoint = await _endpointRepository.GetEndpoint(endpointId);

            if (endpoint == null)
                return Ok(new EndpointDto()
                {
                    Enabled = true,
                    CreatedUtc = DateTime.Now,
                    FromUrl = "posts",
                    MockBehaviour = MockBehaviour.MockOnly,
                    RestType = RestType.GET,
                    QueryParameters = new List<QueryParameterDto>()
                    {
                        new QueryParameterDto()
                        {
                            Name = "test",
                            Value = "123"
                        }
                    },
                    EndpointHeaders = new List<EndpointHeaderDto>()
                    {
                        new EndpointHeaderDto()
                        {
                            Name = "Authorization",
                            Value = "ey..."
                        }
                    }
                });
                //return NotFound(ErrorMessageConstants.EndpointNotFound);
            
            return Ok(endpoint);
        }


        [Route("{code:regex(^[[tgm]]{{1,3}}$)}/{**rest}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EndpointDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<EndpointDto>> CreateRequest(string code, string rest, [FromBody] EndpointDto? endpointDto)
        {
            if (!HelperExtensions.TryParseParamCodes(code, rest,
                      out var tenantPath, out var environmentPath,
                      out var microservicePath, out var endpointUrl, out var error))
            {
                return BadRequest(error);
            }
                        
            if (endpointDto == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            var results = new List<ValidationResult>();

            bool isValid = GeneralHelper.TryValidateFullObject(endpointDto, new ValidationContext(endpointDto, null), results);

            await _endpointRepository.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(tenantPath, environmentPath, microservicePath);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());
            
            var createdRequest =
                await _endpointRepository.CreateEndpointAsync(endpointDto);

            if (createdRequest == null)
                return NotFound(ErrorMessageConstants.MicroserviceNotFound);
            
            return StatusCode(201, createdRequest);
        }

        [HttpPut("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateEndpointDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<UpdateEndpointDto>> UpdateRequest(int endpointId, [FromBody] UpdateEndpointDto? updatedEndpoint)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            if (updatedEndpoint == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            var results = new List<ValidationResult>();

            bool isValid = GeneralHelper.TryValidateFullObject(updatedEndpoint, new ValidationContext(updatedEndpoint, null), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());
            
            var updatedRequest = await _endpointRepository.UpdateEndpoint(endpointId, updatedEndpoint);

            if (updatedRequest == null)
                return NotFound(ErrorMessageConstants.EndpointNotFound);
            
            return Ok(updatedRequest);
        }

        [HttpPut("responses/{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MockResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpdateMockResponses(int endpointId, [FromBody] List<MockResponseDto>? responses)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            if (responses == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            var results = new List<ValidationResult>();

            foreach (var response in responses)
            {
                GeneralHelper.TryValidateFullObject(response, new ValidationContext(response, null), results);
            }

            if (results.Any())
                return BadRequest(results.ToBadRequestResult());
            
            var updatedResponses = await _endpointRepository.UpdateMockResponses(endpointId, responses);
            
            if(updatedResponses == null)
                return NotFound(ErrorMessageConstants.EndpointNotFound);
            
            return Ok(updatedResponses);
        }

        [HttpPatch("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateEndpointDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<UpdateEndpointDto>> PatchEndpoint(int endpointId, [FromBody] JsonPatchDocument<UpdateEndpointDto>? updatedEndpoint)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            var endpointDto = await _endpointRepository.GetUpdateEndpoint(endpointId);

            if (endpointDto == null || updatedEndpoint == null)
                return NotFound(ErrorMessageConstants.EndpointNotFound);
            
            _logger.LogInformation("Patching endpoint");
            updatedEndpoint.ApplyTo(endpointDto);
            
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(endpointDto, new ValidationContext(endpointDto, null), results, true);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedRequest = await _endpointRepository.UpdateEndpoint(endpointId, endpointDto);

            if (updatedRequest == null)
                return NotFound(ErrorMessageConstants.MicroserviceNotFound);
            
            return Ok(updatedRequest);
        }

        [HttpDelete("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteRequest(int endpointId)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
                    
            return await _endpointRepository.DeleteEndpoint(endpointId) ? Ok() : NoContent();
        }
    }
}
