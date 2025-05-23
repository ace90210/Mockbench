using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Tenant; // For TenantNameList
using Mockbench.Shared.Models.Utility;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Mockbench.Api.Tests.Controllers.Admin;

public class UtilitiesControllerTests
{
    private readonly Mock<ILogger<UtilitiesController>> _mockLogger;
    private readonly Mock<ICommonRepository> _mockCommonRepository;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly UtilitiesController _controller;

    public UtilitiesControllerTests()
    {
        _mockLogger = new Mock<ILogger<UtilitiesController>>();
        _mockCommonRepository = new Mock<ICommonRepository>();
        _mockTenantRepository = new Mock<ITenantRepository>();
        _controller = new UtilitiesController(_mockLogger.Object, _mockCommonRepository.Object, _mockTenantRepository.Object);
    }

    // Constructor Tests
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("logger", () => new UtilitiesController(null!, _mockCommonRepository.Object, _mockTenantRepository.Object));
    }

    [Fact]
    public void Constructor_NullCommonRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("baseRepository", () => new UtilitiesController(_mockLogger.Object, null!, _mockTenantRepository.Object));
    }
    // Note: TenantRepository can be null as per current constructor, if it was mandatory, a test would be here.


    // TestUrl Tests
    [Fact]
    public async Task TestUrl_NullTestObject_ReturnsOkWithUnknownResult()
    {
        // Act
        var result = await _controller.TestUrl(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pingResult = Assert.IsType<PingTestResult>(okResult.Value);
        Assert.Equal(TestUrlResult.Unknown, pingResult.TestUrlResult);
        Assert.Equal("invalid test request", pingResult.Message);
    }

    [Fact]
    public async Task TestUrl_PingOnly_SuccessfulPing_ReturnsOkWithPassedResult()
    {
        // Arrange
        // This test relies on "example.com" being a consistently pingable address.
        // For more robust tests, Ping would need to be abstracted and mocked.
        var testUrl = new TestUrl { Url = "http://example.com", PingOnly = true };

        // Act
        var result = await _controller.TestUrl(testUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pingTestResult = Assert.IsType<PingTestResult>(okResult.Value);
        Assert.Equal(TestUrlResult.Passed, pingTestResult.TestUrlResult);
        Assert.Contains("Ping test passed in", pingTestResult.Message);
    }

    [Fact]
    public async Task TestUrl_PingOnly_FailedPing_ReturnsOkWithFailedResult()
    {
        // Arrange
        // Using a non-existent or unpingable host.
        var testUrl = new TestUrl { Url = "http://nonexistentdomain123456789.com", PingOnly = true };

        // Act
        var actionResult = await _controller.TestUrl(testUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var pingTestResult = Assert.IsType<PingTestResult>(okResult.Value);
        Assert.Equal(TestUrlResult.Failed, pingTestResult.TestUrlResult);
        Assert.Contains("Ping test failed due to:", pingTestResult.Message); // Catches PingException
    }


    [Fact]
    public async Task TestUrl_FullTest_SuccessfulGet_ReturnsOkWithPassedResult()
    {
        // Arrange
        // This test relies on "http://example.com" being accessible.
        // For more robust tests, HttpClient would need to be abstracted/mocked.
        var testUrl = new TestUrl { Url = "http://example.com", PingOnly = false };

        // Act
        var result = await _controller.TestUrl(testUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pingTestResult = Assert.IsType<PingTestResult>(okResult.Value);
        Assert.Equal(TestUrlResult.Passed, pingTestResult.TestUrlResult);
        Assert.Contains("Test passed in", pingTestResult.Message);
    }

    [Fact]
    public async Task TestUrl_FullTest_FailedGet_ReturnsOkWithFailedResult()
    {
        // Arrange
        var testUrl = new TestUrl { Url = "http://nonexistentdomain123456789.com/path", PingOnly = false };

        // Act
        var result = await _controller.TestUrl(testUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pingTestResult = Assert.IsType<PingTestResult>(okResult.Value);
        Assert.Equal(TestUrlResult.Failed, pingTestResult.TestUrlResult);
        // The exact message can vary ("No such host is known", "Name or service not known", or status code for other errors)
        Assert.True(pingTestResult.Message.Contains("Test failed due to:") || pingTestResult.Message.Contains("Test failed:"));
    }

    // ExportDatabaseAsFile Tests
    [Fact]
    public async Task ExportDatabaseAsFile_ReturnsFileContentResult()
    {
        // Arrange
        var fullDatabaseDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto>() };
        _mockCommonRepository.Setup(r => r.ExportDatabaseToJson()).ReturnsAsync(fullDatabaseDto);

        // Act
        var result = await _controller.ExportDatabaseAsFile();

        // Assert
        Assert.IsType<FileContentResult>(result);
        var fileResult = (FileContentResult)result;
        Assert.Equal("application/json", fileResult.ContentType);
        Assert.Equal("Mockbench-backup.json", fileResult.FileDownloadName);

        var json = Encoding.UTF8.GetString(fileResult.FileContents);
        var deserializedResult = JsonConvert.DeserializeObject<FullDatabaseDto>(json);
        Assert.NotNull(deserializedResult);
    }

    // ExportDatabaseAsJson Tests
    [Fact]
    public async Task ExportDatabaseAsJson_ReturnsOkWithData()
    {
        // Arrange
        var expectedDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "Test Tenant" } } };
        _mockCommonRepository.Setup(r => r.ExportDatabaseToJson()).ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.ExportDatabaseAsJson();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualDto = Assert.IsType<FullDatabaseDto>(okResult.Value);
        Assert.Equal(expectedDto.Tenants.Count(), actualDto.Tenants.Count());
        Assert.Equal(expectedDto.Tenants.First().Name, actualDto.Tenants.First().Name);
    }

    // ImportDatabaseAsJson Tests
    [Fact]
    public async Task ImportDatabaseAsJson_NullImport_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ImportDatabaseAsJson(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorDto = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
        var errorMessage = Assert.Single(errorDto.Errors);
        Assert.Contains(ErrorMessageConstants.InvalidOrMissingBody, errorMessage.Value);
    }

    [Fact]
    public async Task ImportDatabaseAsJson_ValidImport_ReturnsOk()
    {
        // Arrange
        var importDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "NewValidTenant", Path = "newvalidtenant" } } };
        _mockTenantRepository.Setup(r => r.GetAllTakenTenantNameAndPathsAsync()).ReturnsAsync(new List<PathNameItem>());
        _mockCommonRepository.Setup(r => r.ImportDatabase(importDto, false)).ReturnsAsync(true);

        // Act
        var result = await _controller.ImportDatabaseAsJson(importDto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task ImportDatabaseAsJson_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        // Example: Missing required field if TenantBaseDto had validation attributes like [Required]
        var importDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = null } } }; // Assuming Name is required
        _mockTenantRepository.Setup(r => r.GetAllTakenTenantNameAndPathsAsync()).ReturnsAsync(new List<PathNameItem>());
        // GeneralHelper.TryValidateFullObject will return false and populate results

        // Act
        var result = await _controller.ImportDatabaseAsJson(importDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorDto = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
        Assert.NotEmpty(errorDto.Errors); // Check that there are validation errors
    }

    [Fact]
    public async Task ImportDatabaseAsJson_DuplicateTenantPath_ReturnsBadRequest()
    {
        // Arrange
        var existingTenants = new List<PathNameItem> { new PathNameItem { Path = "/existingtenant" } };
        var importDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "New Tenant", Path = "/existingtenant" } } };
        _mockTenantRepository.Setup(r => r.GetAllTakenTenantNameAndPathsAsync()).ReturnsAsync(existingTenants);

        // Act
        var result = await _controller.ImportDatabaseAsJson(importDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorDto = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
        Assert.NotEmpty(errorDto.Errors);
        // You might want to check for a specific error message if your validation provides one
        // For example: Assert.Contains("Path '/existingtenant' is already taken.", errorDto.Errors.First());
    }


    [Fact]
    public async Task ImportDatabaseAsJson_ImportFailsInRepository_ReturnsBadRequest()
    {
        // Arrange
        var importDto = new FullDatabaseDto { Tenants = new List<TenantBaseDto> { new TenantBaseDto() { Name = "ValidTenant", Path = "validtenant" } } };
        _mockTenantRepository.Setup(r => r.GetAllTakenTenantNameAndPathsAsync()).ReturnsAsync(new List<PathNameItem>());
        _mockCommonRepository.Setup(r => r.ImportDatabase(importDto, false)).ReturnsAsync(false);

        // Act
        var result = await _controller.ImportDatabaseAsJson(importDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorDto = Assert.IsType<BadRequestResultDto>(badRequestResult.Value);
        var errorMessage = Assert.Single(errorDto.Errors);
        Assert.Contains(ErrorMessageConstants.TenantNotFound, errorMessage.Value);
    }
}
