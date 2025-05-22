using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Utility; 
using Moq;

namespace Mockbench.Api.Tests.Admin;

public class MicroserviceControllerTests
{
    private readonly Mock<ILogger<MicroserviceController>> _loggerMock;
    private readonly Mock<IMicroserviceRepository> _microserviceRepositoryMock;
    private readonly MicroserviceController _controller;

    public MicroserviceControllerTests()
    {
        _loggerMock = new Mock<ILogger<MicroserviceController>>();
        _microserviceRepositoryMock = new Mock<IMicroserviceRepository>();
        _controller = new MicroserviceController(_loggerMock.Object, _microserviceRepositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Ensures ModelState is available
            }
        };
    }

    // --- GetAll Tests ---
    [Fact]
    public async Task GetAll_ReturnsOkObjectResultWithMicroservices()
    {
        // Arrange
        var expectedMicroservices = new List<MicroserviceDto>
        {
            new MicroserviceDto { Id = 1, Name = "Service1" },
            new MicroserviceDto { Id = 2, Name = "Service2" }
        };
        _microserviceRepositoryMock.Setup(repo => repo.GetMicroservicesAsync())
            .ReturnsAsync(expectedMicroservices);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualMicroservices = Assert.IsAssignableFrom<IEnumerable<MicroserviceDto>>(okResult.Value);
        Assert.Equal(expectedMicroservices.Count, actualMicroservices.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoMicroservicesExist()
    {
        // Arrange
        _microserviceRepositoryMock.Setup(repo => repo.GetMicroservicesAsync())
            .ReturnsAsync(new List<MicroserviceDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualMicroservices = Assert.IsAssignableFrom<IEnumerable<MicroserviceDto>>(okResult.Value);
        Assert.Empty(actualMicroservices);
    }

    // --- GetById Tests ---
    [Fact]
    public async Task GetById_ValidId_ReturnsOkObjectResultWithMicroservice()
    {
        // Arrange
        var microserviceId = 1;
        var expectedMicroservice = new MicroserviceDto { Id = microserviceId, Name = "TestService" };
        _microserviceRepositoryMock.Setup(repo => repo.GetMicroserviceByIdAsync(microserviceId))
            .ReturnsAsync(expectedMicroservice);

        // Act
        var result = await _controller.GetById(microserviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualMicroservice = Assert.IsType<MicroserviceDto>(okResult.Value);
        Assert.Equal(expectedMicroservice.Id, actualMicroservice.Id);
        Assert.Equal(expectedMicroservice.Name, actualMicroservice.Name);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 0;

        // Act
        var result = await _controller.GetById(microserviceId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.MicroserviceIdInvalid, badRequestResult.Value);
    }

    [Fact]
    public async Task GetById_MicroserviceNotFound_ReturnsNotFound()
    {
        // Arrange
        var microserviceId = 1;
        _microserviceRepositoryMock.Setup(repo => repo.GetMicroserviceByIdAsync(microserviceId))
            .ReturnsAsync((MicroserviceDto)null);

        // Act
        var result = await _controller.GetById(microserviceId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.MicroserviceNotFound, notFoundResult.Value);
    }

    // --- Create Tests ---
    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var microserviceDto = new MicroserviceDto { Name = "NewService", Path = "api", ProxyMode = ProxyMode.None }; //
        var createdMicroserviceDto = new MicroserviceDto { Id = 1, Name = "NewService", Path = "/api", ProxyMode = ProxyMode.None};
        _microserviceRepositoryMock.Setup(repo => repo.CreateMicroserviceAsync(microserviceDto))
            .ReturnsAsync(createdMicroserviceDto);

        // Act
        var result = await _controller.Create(microserviceDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedResult>(result.Result);
        var actualDto = Assert.IsType<MicroserviceDto>(createdAtActionResult.Value);
        Assert.Equal($"api/microservice/{actualDto.Id}", createdAtActionResult.Location);
        Assert.Equal(createdMicroserviceDto.Name, actualDto.Name);
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Arrange
        // Act
        var result = await _controller.Create(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task Create_InvalidDto_NameMissing_ReturnsBadRequest()
    {
        // Arrange
        var microserviceDto = new MicroserviceDto (); // Name is required
        _controller.ModelState.AddModelError("Name", "The Name field is required."); // Simulate model validation failure

        // Act
        var result = await _controller.Create(microserviceDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<BadRequestObjectResult>(badRequestResult); // Controller returns ModelState directly which gets converted
    }

    [Fact]
    public async Task Create_RepositoryReturnsNull_ReturnsInternalServerError()
    {
        // Arrange
        var microserviceDto = new MicroserviceDto { Name = "NewService", Path = "test", ProxyMode = ProxyMode.None }; //
        _microserviceRepositoryMock.Setup(repo => repo.CreateMicroserviceAsync(microserviceDto))
            .Throws(new Exception(ErrorMessageConstants.MicroserviceIdInvalid)); // Simulate repository failure

        // Act
        var result = await _controller.Create(microserviceDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal(ErrorMessageConstants.MicroserviceIdInvalid, statusCodeResult.Value);
    }


    // --- Update Tests ---
    [Fact]
    public async Task Update_ValidIdAndDto_ReturnsOkObjectResult()
    {
        // Arrange
        var microserviceId = 1;
        var microserviceDto = new MicroserviceDto { Id = microserviceId, Name = "UpdatedService", Path = "test", ProxyMode = ProxyMode.None }; //
        _microserviceRepositoryMock.Setup(repo => repo.UpdateMicroserviceAsync(microserviceId, microserviceDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(microserviceId, microserviceDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<MicroserviceDto>(okResult.Value);
        Assert.Equal(microserviceDto.Name, actualDto.Name);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 0;
        var microserviceDto = new MicroserviceDto { Name = "Test" }; //

        // Act
        var result = await _controller.Update(microserviceId, microserviceDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.MicroserviceIdInvalid, badRequestResult.Value);
    }

    [Fact]
    public async Task Update_NullDto_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 1;

        // Act
        var result = await _controller.Update(microserviceId, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 1;
        var microserviceDto = new MicroserviceDto { Id = 2, Name = "Test" }; //

        // Act
        var result = await _controller.Update(microserviceId, microserviceDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.IdMissMatch, badRequestResult.Value);
    }

    [Fact]
    public async Task Update_InvalidDto_NameMissing_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 1;
        var microserviceDto = new MicroserviceDto { Id = microserviceId }; // Name is required
        _controller.ModelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = await _controller.Update(microserviceId, microserviceDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequestResult.Value);
    }

    [Fact]
    public async Task Update_RepositoryReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var microserviceId = 1;
        var microserviceDto = new MicroserviceDto { Id = microserviceId, Name = "UpdatedService", Path = "test", ProxyMode = ProxyMode.None }; //
        _microserviceRepositoryMock.Setup(repo => repo.UpdateMicroserviceAsync(microserviceId, microserviceDto))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(microserviceId, microserviceDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }

    // --- Delete Tests ---
    [Fact]
    public async Task Delete_ValidId_RepositoryReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        var microserviceId = 1;
        _microserviceRepositoryMock.Setup(repo => repo.DeleteMicroserviceAsync(microserviceId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(microserviceId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ValidId_RepositoryReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var microserviceId = 1;
        _microserviceRepositoryMock.Setup(repo => repo.DeleteMicroserviceAsync(microserviceId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(microserviceId);

        // Assert
        var notFoundResult = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var microserviceId = 0;

        // Act
        var result = await _controller.Delete(microserviceId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.MicroserviceIdInvalid, badRequestResult.Value);
    }
}
