using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Server.Controllers.MockControllers;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Moq;

namespace Mockbench.Api.Tests;
public class MockControllerTests
{
    private readonly Mock<ILogger<MockController>> _loggerMock;
    private readonly Mock<IEndpointRepository> _endpointRepositoryMock;
    private readonly Mock<IHttpService> _httpServiceMock;
    private readonly MockController _controller;

    public MockControllerTests()
    {
        _loggerMock = new Mock<ILogger<MockController>>();
        _endpointRepositoryMock = new Mock<IEndpointRepository>();
        _httpServiceMock = new Mock<IHttpService>();

        _controller = new MockController(
            _loggerMock.Object,
            _endpointRepositoryMock.Object,
            _httpServiceMock.Object
        );

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task ProxyAsync_InvalidPath_ReturnsNotFound()
    {
        // Arrange
        string code = "xyz";
        string rest = "invalid/rest";
        // Simulate TryParseParamCodes failure
        // Since it's a static method, we'll assume it's already tested independently
        // This test simulates the failure by bypassing the static helper (can't mock directly)

        // Act
        var result = await _controller.ProxyAsync(null, null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ProxyAsync_DisabledEnvironment_ReturnsBadRequest()
    {
        // Arrange
        var matchingEndpoint = new MatchingEndpoints
        {
            Environment = new EnvironmentDto { Enabled = false },
            Microservice = new FullMicroserviceDto { Enabled = true }
        };

        _endpointRepositoryMock
            .Setup(x => x.GetAllMatchingEndpointsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(matchingEndpoint);

        SetRequestMethod("GET");

        // Simulate valid code + rest
        var result = await _controller.ProxyAsync("t", "e/m/s");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.EnvironmentDisabled, badRequestResult.Value);
    }

    [Fact]
    public async Task ProxyAsync_DisabledMicroservice_ReturnsBadRequest()
    {
        // Arrange
        var matchingEndpoint = new MatchingEndpoints
        {
            Environment = new EnvironmentDto { Enabled = true },
            Microservice = new FullMicroserviceDto { Enabled = false }
        };

        _endpointRepositoryMock
            .Setup(x => x.GetAllMatchingEndpointsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(matchingEndpoint);

        SetRequestMethod("GET");

        var result = await _controller.ProxyAsync("t", "e/m/s");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.MicroserviceDisabled, badRequestResult.Value);
    }

    [Fact]
    public async Task ProxyAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var expectedResult = new OkResult();

        var matchingEndpoint = new MatchingEndpoints
        {
            Environment = new EnvironmentDto { Enabled = true },
            Microservice = new FullMicroserviceDto { Enabled = true }
        };

        _endpointRepositoryMock
            .Setup(x => x.GetAllMatchingEndpointsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(matchingEndpoint);

        _httpServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<MatchingEndpoints>(), RestType.GET, It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResult);

        SetRequestMethod("GET");

        // Act
        var result = await _controller.ProxyAsync("t", "e/m/s");

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task ProxyAsync_NullResponse_ReturnsNotFound()
    {
        // Arrange
        var matchingEndpoint = new MatchingEndpoints
        {
            Environment = new EnvironmentDto { Enabled = true },
            Microservice = new FullMicroserviceDto { Enabled = true }
        };

        _endpointRepositoryMock
            .Setup(x => x.GetAllMatchingEndpointsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(matchingEndpoint);

        _httpServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<MatchingEndpoints>(), RestType.GET, It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync((IActionResult?)null);

        SetRequestMethod("GET");

        // Act
        var result = await _controller.ProxyAsync("t", "e/m/s");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    private void SetRequestMethod(string method)
    {
        _controller.ControllerContext.HttpContext.Request.Method = method;
    }
}