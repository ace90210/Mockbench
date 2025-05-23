using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Response;
using Moq;

namespace Mockbench.Api.Tests.Controllers.Admin;

public class MockResponseControllerTests
{
    private readonly Mock<ILogger<MockResponseController>> _loggerMock;
    private readonly Mock<IMockResponseRepository> _mockResponseRepositoryMock;
    private readonly MockResponseController _controller;

    public MockResponseControllerTests()
    {
        _loggerMock = new Mock<ILogger<MockResponseController>>();
        _mockResponseRepositoryMock = new Mock<IMockResponseRepository>();
        _controller = new MockResponseController(_loggerMock.Object, _mockResponseRepositoryMock.Object);
    }

    #region Get Tests

    [Fact]
    public async Task Get_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Get(0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseId, badRequestResult.Value);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockResponseRepositoryMock.Setup(r => r.GetMockResponseAsync(It.IsAny<int>())).ReturnsAsync((MockResponseDto)null);

        // Act
        var result = await _controller.Get(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseNotFound, notFoundResult.Value);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var mockResponse = new MockResponseDto { Id = 1, Description = "Test Response" };
        _mockResponseRepositoryMock.Setup(r => r.GetMockResponseAsync(1)).ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.Get(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResponse = Assert.IsType<MockResponseDto>(okResult.Value);
        Assert.Equal(1, returnedResponse.Id);
    }

    #endregion

    #region CreateResponse Tests

    [Fact]
    public async Task CreateResponse_WithInvalidEndpointId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateResponse(0, new MockResponseDto());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
    }

    [Fact]
    public async Task CreateResponse_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateResponse(1, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task CreateResponse_WhenCreationFails_ReturnsBadRequest()
    {
        // Arrange
        var responseDto = new MockResponseDto { Description = "Test" };
        _mockResponseRepositoryMock.Setup(r => r.CreateAsync(1, responseDto))
            .ReturnsAsync((false, null));

        // Act
        var result = await _controller.CreateResponse(1, responseDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.FailedToCreate, badRequestResult.Value);
    }

    [Fact]
    public async Task CreateResponse_WithValidData_ReturnsCreated()
    {
        // Arrange
        var requestDto = new MockResponseDto { Description = "Test" };
        var createdDto = new MockResponseDto { Id = 1, Description = "Test" };
        _mockResponseRepositoryMock.Setup(r => r.CreateAsync(1, requestDto))
            .ReturnsAsync((true, createdDto));

        // Act
        var result = await _controller.CreateResponse(1, requestDto);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.IsType<MockResponseDto>(createdResult.Value);
    }

    #endregion

    #region PostBulkCreateForRequest Tests (NEW)

    [Fact]
    public async Task PostBulkCreateForRequest_WithInvalidEndpointId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.PostBulkCreateForRequest(0, new List<MockResponseDto>());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
    }

    [Fact]
    public async Task PostBulkCreateForRequest_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.PostBulkCreateForRequest(1, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task PostBulkCreateForRequest_WhenEndpointNotFound_ReturnsNotFound()
    {
        // Arrange
        var responses = new List<MockResponseDto> { new MockResponseDto { Description = "Test" } };
        _mockResponseRepositoryMock.Setup(r => r.CreateBulkAsync(1, responses))
            .ReturnsAsync((false, null)); // This indicates endpoint not found in repo logic

        // Act
        var result = await _controller.PostBulkCreateForRequest(1, responses);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.EndpointNotFound, notFoundResult.Value);
    }

    [Fact]
    public async Task PostBulkCreateForRequest_WithValidData_ReturnsCreated()
    {
        // Arrange
        var requestList = new List<MockResponseDto> { new MockResponseDto { Description = "Test1" }, new MockResponseDto { Description = "Test2" } };
        var createdList = new List<MockResponseDto> { new MockResponseDto { Id = 1, Description = "Test1" }, new MockResponseDto { Id = 2, Description = "Test2" } };

        _mockResponseRepositoryMock.Setup(r => r.CreateBulkAsync(1, requestList))
            .ReturnsAsync((true, createdList));

        // Act
        var result = await _controller.PostBulkCreateForRequest(1, requestList);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedList = Assert.IsType<List<MockResponseDto>>(createdResult.Value);
        Assert.Equal(2, returnedList.Count);
    }
    #endregion

    #region UpdateResponse Tests

    [Fact]
    public async Task UpdateResponse_WithInvalidResponseId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateResponse(0, new MockResponseDto());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseId, badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateResponse_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateResponse(1, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateResponse_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockResponseRepositoryMock.Setup(r => r.UpdateMockResponseAsync(It.IsAny<int>(), It.IsAny<MockResponseDto>()))
            .ReturnsAsync((MockResponseDto)null);

        // Act
        var result = await _controller.UpdateResponse(1, new MockResponseDto { Description = "Update" });

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseNotFound, notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateResponse_WithValidData_ReturnsOk()
    {
        // Arrange
        var responseDto = new MockResponseDto { Description = "Updated" };
        var updatedDto = new MockResponseDto { Id = 1, Description = "Updated" };
        _mockResponseRepositoryMock.Setup(r => r.UpdateMockResponseAsync(1, responseDto)).ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.UpdateResponse(1, responseDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MockResponseDto>(okResult.Value);
    }

    #endregion

    #region DeleteResponse Tests

    [Fact]
    public async Task DeleteResponse_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.DeleteResponse(0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.ResponseId, badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteResponse_WhenDeletionSucceeds_ReturnsOk()
    {
        // Arrange
        _mockResponseRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteResponse(1);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteResponse_WhenDeletionFails_ReturnsNoContent()
    {
        // Arrange
        _mockResponseRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteResponse(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    #endregion

    #region DeleteBulkCreateForRequest Tests (NEW)

    [Fact]
    public async Task DeleteBulkCreateForRequest_WithInvalidRequestId_ReturnsBadRequest()
    {
        // Arrange
        var responses = new List<MockResponseDto> { new MockResponseDto { Id = 1 } };
        // Act
        var result = await _controller.DeleteBulkCreateForRequest(0, responses);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.EndpointId, badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteBulkCreateForRequest_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.DeleteBulkCreateForRequest(1, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteBulkCreateForRequest_WhenDeletionSucceeds_ReturnsOk()
    {
        // Arrange
        var responses = new List<MockResponseDto> { new MockResponseDto { Id = 1 } };
        _mockResponseRepositoryMock.Setup(r => r.DeleteBulkAsync(1, responses)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBulkCreateForRequest(1, responses);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteBulkCreateForRequest_WhenDeletionFails_ReturnsNoContent()
    {
        // Arrange
        var responses = new List<MockResponseDto> { new MockResponseDto { Id = 1 } };
        _mockResponseRepositoryMock.Setup(r => r.DeleteBulkAsync(1, responses)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteBulkCreateForRequest(1, responses);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    #endregion

    #region PatchResponse Tests

    [Fact]
    public async Task PatchResponse_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.PatchResponse(0, new JsonPatchDocument<UpdateMockResponseDto>());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseId, badRequestResult.Value);
    }

    [Fact]
    public async Task PatchResponse_WithNullBody_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.PatchResponse(1, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task PatchResponse_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockResponseRepositoryMock.Setup(r => r.GetUpdateMockResponseAsync(It.IsAny<int>())).ReturnsAsync((UpdateMockResponseDto)null);

        // Act
        var result = await _controller.PatchResponse(1, new JsonPatchDocument<UpdateMockResponseDto>());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.ResponseNotFound, notFoundResult.Value);
    }

    [Fact]
    public async Task PatchResponse_WithValidData_ReturnsOk()
    {
        // Arrange
        var responseToUpdate = new UpdateMockResponseDto { Description = "Original Name" };
        var patchDoc = new JsonPatchDocument<UpdateMockResponseDto>();
        patchDoc.Replace(r => r.Description, "Patched Name");

        _mockResponseRepositoryMock.Setup(r => r.GetUpdateMockResponseAsync(1)).ReturnsAsync(responseToUpdate);
        _mockResponseRepositoryMock.Setup(r => r.PatchMockResponseAsync(1, It.IsAny<UpdateMockResponseDto>()))
            .ReturnsAsync(new UpdateMockResponseDto { Description = "Patched Name" });

        // Act
        var result = await _controller.PatchResponse(1, patchDoc);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var patchedDto = Assert.IsType<UpdateMockResponseDto>(okResult.Value);
        Assert.Equal("Patched Name", patchedDto.Description);
    }

    #endregion
}
