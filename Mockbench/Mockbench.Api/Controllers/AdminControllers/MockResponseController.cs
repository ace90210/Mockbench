using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Response;
using Mockbench.Shared.Models.Utility;
using Swashbuckle.AspNetCore.Annotations;

namespace Mockbench.Api.Controllers.AdminControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MockResponseController : ControllerBase
    {

        private readonly ILogger<MockResponseController> _logger;
        private readonly IMockResponseRepository _mockResponseRepository;

        public MockResponseController(ILogger<MockResponseController> logger, IMockResponseRepository mockResponseRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mockResponseRepository = mockResponseRepository ?? throw new ArgumentNullException(nameof(mockResponseRepository));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MockResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MockResponseDto>> Get(int id)
        {
            if (id <= 0)
                return BadRequest(ErrorMessageConstants.ResponseId);
            
            var response = await _mockResponseRepository.GetMockResponseAsync(id);

            if (response == null)
                return NotFound(ErrorMessageConstants.ResponseNotFound);

            return Ok(response);
        }

        [HttpPost("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MockResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MockResponseDto>> CreateResponse(int endpointId, [FromBody] MockResponseDto? response)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            if (response == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            var results = new List<ValidationResult>();

            bool isValid = GeneralHelper.TryValidateFullObject(response, new ValidationContext(response, null), results);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());
            
            var created = await _mockResponseRepository.CreateAsync(endpointId, response);
            return created.success ? StatusCode(201, created.result) : BadRequest(ErrorMessageConstants.FailedToCreate);
        }

        [HttpPost("bulk/{endpointId}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(List<MockResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<List<MockResponseDto>>> PostBulkCreateForRequest(int endpointId, [FromBody] List<MockResponseDto>? responses)
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
            
            var created = await _mockResponseRepository.CreateBulkAsync(endpointId, responses);
            
            if (created.success)
            {
                return StatusCode(201, created.result);
            }
            else
            {
                if (created.result == null)
                    return NotFound(ErrorMessageConstants.EndpointNotFound);
                
                return BadRequest(ErrorMessageConstants.FailedToCreate);
            }
        }

        [HttpPut("{endpointId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MockResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<MockResponseDto>> UpdateResponseForRequest(int endpointId, [FromBody] MockResponseDto? response)
        {
            if (endpointId <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            if (response == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            var updatedResponse = await _mockResponseRepository.UpdateMockResponseAsync(endpointId, response);

            if (updatedResponse == null)
            {
                return NotFound(ErrorMessageConstants.ResponseNotFound);
            }
            
            return Ok(updatedResponse);
        }

        [HttpDelete("{responseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteResponse(int responseId)
        {
            if (responseId <= 0)
                return BadRequest(ErrorMessageConstants.ResponseId);

            var deleted = await _mockResponseRepository.DeleteAsync(responseId);
            return deleted ? Ok() : NoContent();
        }


        [HttpDelete("bulk/{requestid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteBulkCreateForRequest(int requestid, [FromBody] List<MockResponseDto>? responses)
        {
            if (requestid <= 0)
                return BadRequest(ErrorMessageConstants.EndpointId);
            
            if (responses == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);

            var deleted = await _mockResponseRepository.DeleteBulkAsync(requestid, responses);
            return deleted ? Ok() : NoContent();
        }

        [HttpPatch("{mockResponseId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMockResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<UpdateMockResponseDto>> PatchResponse(int mockResponseId, [FromBody] JsonPatchDocument<UpdateMockResponseDto>? updateResponseDto)
        {
            if (mockResponseId <= 0)
                return BadRequest(ErrorMessageConstants.ResponseId);
            
            if (updateResponseDto == null)
                return BadRequest(ErrorMessageConstants.InvalidOrMissingBody);
            
            var response = await _mockResponseRepository.GetUpdateMockResponseAsync(mockResponseId);

            if (response == null)
                return NotFound(ErrorMessageConstants.ResponseNotFound);
            
            _logger.LogInformation("patching response");
            updateResponseDto.ApplyTo(response);
            
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(response, new ValidationContext(response, null), results, true);

            if (!isValid)
                return BadRequest(results.ToBadRequestResult());

            var result = await _mockResponseRepository.PatchMockResponseAsync(mockResponseId, response);

            if (result == null)
                return NotFound(ErrorMessageConstants.ResponseNotFound);
            
            return Ok(result);
        }
    }
}
