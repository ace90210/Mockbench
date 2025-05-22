using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Response;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;
using Mockbench.Shared.Models.Utility; // Required for BadRequestResultDto
using Microsoft.AspNetCore.Http;       // Required for StatusCodes
using System.Net; // Required for HttpStatusCode
using Mockbench.Shared.Models.Enum; // Required for RestType, MockBehaviour
using Mockbench.Shared.Models.Tenant; // Required for TenantListDto
using Mockbench.Shared.Models.Environment; // Required for EnvironmentDto
using Mockbench.Shared.Models.Microservice; // Required for MicroserviceDto

namespace Mockbench.Api.Tests.Admin
{
    public class EndpointControllerTests
    {
        private readonly Mock<ILogger<EndpointController>> _loggerMock;
        private readonly Mock<IEndpointRepository> _endpointRepositoryMock;
        private readonly EndpointController _controller;

        public EndpointControllerTests()
        {
            _loggerMock = new Mock<ILogger<EndpointController>>();
            _endpointRepositoryMock = new Mock<IEndpointRepository>();
            _controller = new EndpointController(_loggerMock.Object, _endpointRepositoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // Ensures ModelState is available
                }
            };
        }

        // --- GetById Tests ---

        [Fact]
        public async Task GetById_ValidId_ReturnsOkObjectResultWithEndpoint()
        {
            // Arrange
            var endpointId = 1;
            // Corrected: EndpointDto does not have 'Name', using 'FromUrl' as an example property.
            var expectedEndpoint = new EndpointDto { Id = endpointId, FromUrl = "/test/endpoint" };
            _endpointRepositoryMock.Setup(repo => repo.GetEndpoint(endpointId))
                .ReturnsAsync(expectedEndpoint);

            // Act
            var result = await _controller.GetById(endpointId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualEndpoint = Assert.IsType<EndpointDto>(okResult.Value);
            Assert.Equal(expectedEndpoint.Id, actualEndpoint.Id);
            // Corrected: Asserting 'FromUrl'.
            Assert.Equal(expectedEndpoint.FromUrl, actualEndpoint.FromUrl);
        }

        [Fact]
        public async Task GetById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 0;

            // Act
            var result = await _controller.GetById(endpointId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
        }

        [Fact]
        public async Task GetById_EndpointNotFound_ReturnsNotFound()
        {
            // Arrange
            var endpointId = 1;
            _endpointRepositoryMock.Setup(repo => repo.GetEndpoint(endpointId))
                .ReturnsAsync((EndpointDto)null);

            // Act
            var result = await _controller.GetById(endpointId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value);
        }

        // --- CreateEndpoint Tests ---
        [Fact]
        public async Task CreateEndpoint_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            // Corrected: EndpointDto uses FromUrl, RestType.
            var endpointDto = new EndpointDto
            {
                FromUrl = "/new/endpoint",
                RestType = RestType.POST,
                MicroserviceId = 1, // Assuming a valid MicroserviceIdInvalid
                // FromUrl is [Required(AllowEmptyStrings = true)], so providing it.
                // Other properties will use defaults or can be null.
            };
            // Corrected: EndpointDto uses FromUrl, RestType.
            var createdEndpointDto = new EndpointDto { Id = 123, FromUrl = endpointDto.FromUrl, RestType = endpointDto.RestType, MicroserviceId = endpointDto.MicroserviceId };

            _endpointRepositoryMock.Setup(repo => repo.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MatchingEndpoints { Tenant = new TenantBase { Id = 1 }, Environment = new EnvironmentDto { Id = 1 }, Microservice = new MicroserviceDto { Id = 1 } });

            _endpointRepositoryMock.Setup(repo => repo.CreateEndpointAsync(It.IsAny<EndpointDto>()))
                .ReturnsAsync(createdEndpointDto);

            // Act
            var result = await _controller.CreateEndpoint("t", "e/m/s", endpointDto);

            // Assert
            // Controller returns StatusCode(201, createdRequest)
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);
            var actualDto = Assert.IsType<EndpointDto>(statusCodeResult.Value);
            Assert.Equal(createdEndpointDto.Id, actualDto.Id);
            Assert.Equal(createdEndpointDto.FromUrl, actualDto.FromUrl);
        }

        [Fact]
        public async Task CreateEndpoint_NullBody_ReturnsBadRequest()
        {
            // Arrange
            // Act
            var result = await _controller.CreateEndpoint("t", "e/m/s", null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
        }

        [Fact]
        public async Task CreateEndpoint_InvalidPath_ReturnsBadRequest()
        {
            // Arrange
            // Corrected: EndpointDto requires FromUrl.
            var endpointDto = new EndpointDto() { FromUrl = new string('a', 5001) };

            // Act: Providing a path that HelperExtensions.TryParseParamCodes would fail for
            var result = await _controller.CreateEndpoint(null, "invalid", endpointDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult.Value); // Error message comes from HelperExtensions
        }


        // --- UpdateRequest Tests ---
        [Fact]
        public async Task UpdateRequest_ValidIdAndDto_ReturnsOkObjectResult()
        {
            // Arrange
            var endpointId = 1;
            // Corrected: UpdateEndpointDto uses FromUrl, RestType etc.
            var updateDto = new UpdateEndpointDto { FromUrl = "/updated/url", RestType = RestType.PUT, Enabled = true };
            _endpointRepositoryMock.Setup(repo => repo.UpdateEndpoint(endpointId, It.IsAny<UpdateEndpointDto>()))
                .ReturnsAsync(updateDto); // Assuming UpdateEndpoint returns the DTO passed or a mapped one

            // Act
            var result = await _controller.UpdateRequest(endpointId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualDto = Assert.IsType<UpdateEndpointDto>(okResult.Value);
            Assert.Equal(updateDto.FromUrl, actualDto.FromUrl);
        }

        [Fact]
        public async Task UpdateRequest_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 0;
            var updateDto = new UpdateEndpointDto { FromUrl = "/updated/url", RestType = RestType.PUT };


            // Act
            var result = await _controller.UpdateRequest(endpointId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRequest_NullDto_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 1;

            // Act
            var result = await _controller.UpdateRequest(endpointId, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRequest_EndpointNotFound_ReturnsNotFound()
        {
            // Arrange
            var endpointId = 1;
            var updateDto = new UpdateEndpointDto { FromUrl = "/updated/url", RestType = RestType.PUT };
            _endpointRepositoryMock.Setup(repo => repo.UpdateEndpoint(endpointId, It.IsAny<UpdateEndpointDto>()))
                .ReturnsAsync((UpdateEndpointDto)null);

            // Act
            var result = await _controller.UpdateRequest(endpointId, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value);
        }

        // --- UpdateMockResponses Tests ---
        [Fact]
        public async Task UpdateMockResponses_ValidRequest_ReturnsOkWithResponses()
        {
            // Arrange

            var endpointDto = new EndpointDto { Id = 1, FromUrl = "/test" };

            // Corrected: MockResponseDto uses Description and Code.
            var mockResponses = new List<MockResponseDto> { new MockResponseDto { Description = "Response1", StatusCode = HttpStatusCode.OK, Body = "{}" } };

            // The repository method `UpdateMockResponses` returns `Task<List<MockResponseDto>?>`
            // The controller then returns `Ok(updatedResponses)`
            _endpointRepositoryMock.Setup(repo => repo.UpdateMockResponses(endpointDto.Id, It.IsAny<List<MockResponseDto>>()))
                .Callback<int, List<MockResponseDto>>((id, mockResponses) => endpointDto.MockResponses = mockResponses )
                .ReturnsAsync(endpointDto);

            // Act
            var result = await _controller.UpdateMockResponses(endpointDto.Id, mockResponses);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Result is ActionResult
            var actualResponses = Assert.IsType<EndpointDto>(okResult.Value);
            Assert.Equal(mockResponses, actualResponses.MockResponses);
            Assert.Equal(mockResponses[0].Description, actualResponses.MockResponses[0].Description);
        }

        [Fact]
        public async Task UpdateMockResponses_InvalidEndpointId_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 0;
            // Corrected: MockResponseDto uses Description.
            var mockResponses = new List<MockResponseDto> { new MockResponseDto { Description = "Response1" } };


            // Act
            var result = await _controller.UpdateMockResponses(endpointId, mockResponses);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMockResponses_NullResponses_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 1;

            // Act
            var result = await _controller.UpdateMockResponses(endpointId, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMockResponses_EndpointNotFound_ReturnsNotFound()
        {
            // Arrange
            var endpointDto = new EndpointDto { Id = 1, FromUrl = "/test" };


            // Corrected: MockResponseDto uses Description and Code.
            var mockResponses = new List<MockResponseDto> { new MockResponseDto { Description = "Response1", StatusCode = HttpStatusCode.OK, Body = "{}" } };
            _endpointRepositoryMock.Setup(repo => repo.UpdateMockResponses(endpointDto.Id, It.IsAny<List<MockResponseDto>>()))
                .ReturnsAsync((EndpointDto)null);

            // Act
            var result = await _controller.UpdateMockResponses(endpointDto.Id, mockResponses);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value);
        }


        // --- PatchEndpoint Tests ---
        [Fact]
        public async Task PatchEndpoint_ValidRequest_ReturnsOkWithUpdatedDto()
        {
            // Arrange
            var endpointDto = new EndpointDto { Id = 1, FromUrl = "/test" };

            // Corrected: UpdateEndpointDto uses FromUrl etc.
            var existingEndpointDto = new UpdateEndpointDto { FromUrl = "Original/Url", Enabled = true, RestType = RestType.GET };
            var patchDoc = new JsonPatchDocument<UpdateEndpointDto>();
            patchDoc.Replace(e => e.FromUrl, "Patched/Url"); // Corrected property
            patchDoc.Replace(e => e.Enabled, false);

            _endpointRepositoryMock.Setup(repo => repo.GetUpdateEndpoint(endpointDto.Id))
                .ReturnsAsync(existingEndpointDto);

            // Simulate repository update. The returned DTO should reflect the patch.
            // Note: UpdateEndpointDto does not have an Id property.
            _endpointRepositoryMock.Setup(repo => repo.UpdateEndpoint(endpointDto.Id, It.Is<UpdateEndpointDto>(dto => dto.FromUrl == "Patched/Url" && !dto.Enabled)))
                .ReturnsAsync((int epId, UpdateEndpointDto dto) => new UpdateEndpointDto { FromUrl = dto.FromUrl, Enabled = dto.Enabled, RestType = dto.RestType /* other props copied */ });


            // Act
            var result = await _controller.PatchEndpoint(endpointDto.Id, patchDoc);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedDto = Assert.IsType<UpdateEndpointDto>(okResult.Value);
            // Corrected assertions
            Assert.Equal("Patched/Url", updatedDto.FromUrl);
            Assert.False(updatedDto.Enabled);
        }

        [Fact]
        public async Task PatchEndpoint_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 0;
            var patchDoc = new JsonPatchDocument<UpdateEndpointDto>();

            // Act
            var result = await _controller.PatchEndpoint(endpointId, patchDoc);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
        }

        [Fact]
        public async Task PatchEndpoint_GetUpdateEndpointReturnsNull_ReturnsNotFound()
        {
            // Arrange
            var endpointId = 1;
            _endpointRepositoryMock.Setup(repo => repo.GetUpdateEndpoint(endpointId))
                .ReturnsAsync((UpdateEndpointDto)null); // Simulate endpoint to patch not found
            var patchDoc = new JsonPatchDocument<UpdateEndpointDto>();


            // Act
            var result = await _controller.PatchEndpoint(endpointId, patchDoc);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value); // Message when GetUpdateEndpoint is null
        }

        [Fact]
        public async Task PatchEndpoint_NullPatchDocument_ReturnsNotFound() // Controller treats null patch doc as endpoint not found
        {
            // Arrange
            var endpointId = 1;
            var existingEndpointDto = new UpdateEndpointDto { FromUrl = "Original/Url" }; //
            _endpointRepositoryMock.Setup(repo => repo.GetUpdateEndpoint(endpointId))
                .ReturnsAsync(existingEndpointDto); // Endpoint exists

            // Act
            var result = await _controller.PatchEndpoint(endpointId, null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            // The controller's logic returns EndpointNotFound if updatedEndpoint (patch doc) is null.
            Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value);
        }

        [Fact]
        public async Task PatchEndpoint_UpdateEndpointReturnsNull_ReturnsMicroserviceNotFound() // This is what the controller returns in this specific scenario
        {
            // Arrange
            var endpointId = 1;
            var existingDto = new UpdateEndpointDto { FromUrl = "/original" }; //
            var patchDoc = new JsonPatchDocument<UpdateEndpointDto>();
            patchDoc.Replace(e => e.FromUrl, "/patched");

            _endpointRepositoryMock.Setup(repo => repo.GetUpdateEndpoint(endpointId)).ReturnsAsync(existingDto);
            _endpointRepositoryMock.Setup(repo => repo.UpdateEndpoint(endpointId, It.IsAny<UpdateEndpointDto>()))
                                 .ReturnsAsync((UpdateEndpointDto)null); // Simulate final update failing in repo

            // Act
            var result = await _controller.PatchEndpoint(endpointId, patchDoc);

            // Assert
            // Controller code returns MicroserviceNotFound if the final UpdateEndpoint call returns null
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorMessageConstants.MicroserviceNotFound, notFoundResult.Value);
        }


        // --- DeleteRequest Tests ---
        [Fact]
        public async Task DeleteRequest_ValidId_RepositoryReturnsTrue_ReturnsOk()
        {
            // Arrange
            var endpointId = 1;
            _endpointRepositoryMock.Setup(repo => repo.DeleteEndpoint(endpointId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteRequest(endpointId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteRequest_ValidId_RepositoryReturnsFalse_ReturnsNoContent()
        {
            // Arrange
            var endpointId = 1;
            _endpointRepositoryMock.Setup(repo => repo.DeleteEndpoint(endpointId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteRequest(endpointId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRequest_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpointId = 0;

            // Act
            var result = await _controller.DeleteRequest(endpointId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
        }
    }
}