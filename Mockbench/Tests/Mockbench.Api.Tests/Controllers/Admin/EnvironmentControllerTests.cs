using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.General;
using Moq;

namespace Mockbench.Api.Tests.Controllers.Admin;

public class EnvironmentControllerTests
{
    private readonly Mock<IEnvironmentRepository> _mockRepo;
    private readonly Mock<ILogger<EnvironmentController>> _mockLogger;
    private readonly EnvironmentController _controller;

    public EnvironmentControllerTests()
    {
        _mockRepo = new Mock<IEnvironmentRepository>();
        _mockLogger = new Mock<ILogger<EnvironmentController>>();
        _controller = new EnvironmentController(_mockLogger.Object, _mockRepo.Object);
    }

    [Fact]
    public async Task Get_ReturnsOkResult_WithListOfEnvironments()
    {
        // Arrange
        var environments = new List<EnvironmentDto>
        {
            new EnvironmentDto { Id = 1, Name = "Development" },
            new EnvironmentDto { Id = 2, Name = "Production" }
        };
        _mockRepo.Setup(repo => repo.GetEnvironments()).ReturnsAsync(environments);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<EnvironmentDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetEnvironmentById_ReturnsOkResult_WhenEnvironmentExists()
    {
        // Arrange
        var environment = new EnvironmentDto { Id = 1, Name = "Development" };
        _mockRepo.Setup(repo => repo.GetEnvironmentById(1)).ReturnsAsync(environment);

        // Act
        var result = await _controller.GetEnvironmentById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<EnvironmentDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task GetEnvironmentById_ReturnsNotFound_WhenEnvironmentDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetEnvironmentById(1)).ReturnsAsync((EnvironmentDto)null);

        // Act
        var result = await _controller.GetEnvironmentById(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetEnvironmentById_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.GetEnvironmentById(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateEnvironment_ReturnsCreatedResult_WhenSuccessful()
    {
        // Arrange
        var newEnvironment = new EnvironmentDto { Name = "Staging", Path = "staging" };
        var createdEnvironment = new EnvironmentDto { Id = 3, Name = "Staging", Path = "/staging" };
        _mockRepo.Setup(repo => repo.CreateEnvironment(newEnvironment)).ReturnsAsync(createdEnvironment);
        _mockRepo.Setup(repo => repo.GetAllEnvironmentNameAndPaths()).ReturnsAsync(new List<PathNameItem>());

        // Act
        var result = await _controller.CreateEnvironment(newEnvironment);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateEnvironment_ReturnsBadRequest_WhenEnvironmentIsNull()
    {
        // Act
        var result = await _controller.CreateEnvironment(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateEnvironment_ReturnsOkResult_WhenSuccessful()
    {
        // Arrange
        var updatedEnvironment = new EnvironmentDto { Id = 1, Name = "Development Env", Path = "testpath"};
        _mockRepo.Setup(repo => repo.UpdateEnvironmentBaseValues(updatedEnvironment)).ReturnsAsync(true);
        _mockRepo.Setup(repo => repo.GetAllEnvironmentNameAndPaths(1)).ReturnsAsync(new List<PathNameItem>());


        // Act
        var result = await _controller.UpdateEnvironment(1, updatedEnvironment);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<EnvironmentDto>(okResult.Value);
        Assert.Equal("Development Env", returnValue.Name);
    }

    [Fact]
    public async Task UpdateEnvironment_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.UpdateEnvironment(0, new EnvironmentDto());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateEnvironment_ReturnsNotFound_WhenEnvironmentNotFound()
    {
        // Arrange
        var updatedEnvironment = new EnvironmentDto { Id = 1, Name = "Development Env", Path = "testpath"};
        _mockRepo.Setup(repo => repo.UpdateEnvironmentBaseValues(updatedEnvironment)).ReturnsAsync(false);
        _mockRepo.Setup(repo => repo.GetAllEnvironmentNameAndPaths(1)).ReturnsAsync(new List<PathNameItem>());


        // Act
        var result = await _controller.UpdateEnvironment(1, updatedEnvironment);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteEnvironment_ReturnsOkResult_WhenSuccessful()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.DeleteEnvironment(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteEnvironment(1);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteEnvironment_ReturnsNoContent_WhenEnvironmentNotFound()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.DeleteEnvironment(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteEnvironment(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteEnvironment_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.DeleteEnvironment(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
