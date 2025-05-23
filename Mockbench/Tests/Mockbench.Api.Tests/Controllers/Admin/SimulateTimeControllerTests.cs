using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Services;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Timetravel;
using Moq;

namespace Mockbench.Api.Tests.Controllers.Admin;

public class SimulateTimeControllerTests
{
    private readonly Mock<ILogger<SimulateTimeController>> _mockLogger;
    private readonly Mock<ISimulateTimeService> _mockSimulateTimeService;
    private readonly SimulateTimeController _controller;

    public SimulateTimeControllerTests()
    {
        _mockLogger = new Mock<ILogger<SimulateTimeController>>();
        _mockSimulateTimeService = new Mock<ISimulateTimeService>();
        _controller = new SimulateTimeController(_mockLogger.Object, _mockSimulateTimeService.Object);
    }

    // Tests for SetSimulateAsync
    [Fact]
    public async Task SetSimulateAsync_WithValidIdAndDto_ReturnsOkResult()
    {
        // Arrange
        var id = 1;
        var updateTimeTravelDto = new UpdateTimeTravelDto { Time = DateTime.UtcNow, Scope = TimeTravelScope.Tenant };
        _mockSimulateTimeService.Setup(s => s.SetSimulateTime(updateTimeTravelDto, id)).ReturnsAsync(true);

        // Act
        var result = await _controller.SetSimulateAsync(id, updateTimeTravelDto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SetSimulateAsync_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var id = 0; // Invalid ID
        var updateTimeTravelDto = new UpdateTimeTravelDto { Time = DateTime.UtcNow, Scope = TimeTravelScope.Tenant };

        // Act
        var result = await _controller.SetSimulateAsync(id, updateTimeTravelDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.ScopeId, badRequestResult.Value);
    }

    [Fact]
    public async Task SetSimulateAsync_WithNullDto_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        UpdateTimeTravelDto? updateTimeTravelDto = null;

        // Act
        var result = await _controller.SetSimulateAsync(id, updateTimeTravelDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.SimulateTimeModelWasNotProvided, badRequestResult.Value);
    }

    [Fact]
    public async Task SetSimulateAsync_WhenServiceReturnsFalse_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updateTimeTravelDto = new UpdateTimeTravelDto { Time = DateTime.UtcNow, Scope = TimeTravelScope.Tenant };
        _mockSimulateTimeService.Setup(s => s.SetSimulateTime(updateTimeTravelDto, id)).ReturnsAsync(false);

        // Act
        var result = await _controller.SetSimulateAsync(id, updateTimeTravelDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.InvalidTimeScopeType, badRequestResult.Value);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>("logger", () => new SimulateTimeController(null!, _mockSimulateTimeService.Object));
    }

    [Fact]
    public void Constructor_NullSimulateTimeService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>("simulateTimeService", () => new SimulateTimeController(_mockLogger.Object, null!));
    }

    // Tests for GetTimesAsync
    [Fact]
    public async Task GetTimesAsync_WithValidScopeAndId_ReturnsOkResultWithData()
    {
        // Arrange
        var scope = TimeTravelScope.Environment;
        var id = 1;
        var expectedTimeTravelDto = new TimeTravelDto { CurrentTime = DateTime.UtcNow, AvailableTimes = new List<DateTime> { DateTime.UtcNow } };
        _mockSimulateTimeService.Setup(s => s.GetTimes(scope, id)).ReturnsAsync(expectedTimeTravelDto);

        // Act
        var result = await _controller.GetTimesAsync(scope, id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualTimeTravelDto = Assert.IsType<TimeTravelDto>(okResult.Value);
        Assert.Equal(expectedTimeTravelDto.CurrentTime, actualTimeTravelDto.CurrentTime);
        Assert.Equal(expectedTimeTravelDto.AvailableTimes, actualTimeTravelDto.AvailableTimes);
    }

    [Fact]
    public async Task GetTimesAsync_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var scope = TimeTravelScope.Environment;
        var id = 0; // Invalid ID

        // Act
        var result = await _controller.GetTimesAsync(scope, id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.ScopeId, badRequestResult.Value);
    }

    [Fact]
    public async Task GetTimesAsync_WhenServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var scope = TimeTravelScope.Microservice;
        var id = 1;
        _mockSimulateTimeService.Setup(s => s.GetTimes(scope, id)).ReturnsAsync((TimeTravelDto?)null);

        // Act
        var result = await _controller.GetTimesAsync(scope, id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.InvalidTimeScopeType, badRequestResult.Value);
    }
}
