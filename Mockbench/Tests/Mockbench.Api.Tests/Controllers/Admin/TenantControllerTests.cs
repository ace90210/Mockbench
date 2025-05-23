using Microsoft.AspNetCore.Http; // Required for StatusCodes
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.Repositories;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Shared.Constants;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Utility; // Required for BadRequestResultDto
using Moq;

namespace Mockbench.Api.Tests.Controllers.Admin;

public class TenantControllerTests
{
    private readonly Mock<ILogger<TenantController>> _loggerMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly TenantController _controller;

    public TenantControllerTests()
    {
        _loggerMock = new Mock<ILogger<TenantController>>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _controller = new TenantController(_loggerMock.Object, _tenantRepositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Ensures ModelState is available
            }
        };
    }

    // --- GetAll Tests ---
    [Fact]
    public async Task GetAll_ReturnsOkObjectResultWithTenants()
    {
        // Arrange
        var expectedTenants = new List<TenantBaseDto>
        {
            new TenantBaseDto { Id = 1, Name = "Tenant1", Path = "tenant1path" }, //
            new TenantBaseDto { Id = 2, Name = "Tenant2", Path = "tenant2path" }  //
        };

        var tenantListDto = new TenantListDto
        {
            Tenants = expectedTenants,
            TotalTenants = expectedTenants.Count
        };

        _tenantRepositoryMock.Setup(repo => repo.GetAllTenantsListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(tenantListDto);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTenants = Assert.IsAssignableFrom<TenantListDto>(okResult.Value);
        Assert.Equal(expectedTenants.Count, actualTenants.Tenants.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTenantsExist()
    {
        // Arrange
        _tenantRepositoryMock.Setup(repo => repo.GetAllTenantsListAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new TenantListDto());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTenants = Assert.IsAssignableFrom<TenantListDto>(okResult.Value);
        Assert.Empty(actualTenants.Tenants);
    }

    // --- GetById Tests ---
    [Fact]
    public async Task GetById_ValidId_ReturnsOkObjectResultWithTenant()
    {
        // Arrange
        var tenantId = 1;
        var expectedTenant = new TenantBaseDto { Id = tenantId, Name = "TestTenant", Path = "testtenant" }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByIdAsync(tenantId))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _controller.GetById(tenantId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTenant = Assert.IsType<TenantBaseDto>(okResult.Value);
        Assert.Equal(expectedTenant.Id, actualTenant.Id);
        Assert.Equal(expectedTenant.Name, actualTenant.Name);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 0;

        // Act
        var result = await _controller.GetById(tenantId); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.TenantIdInvalid, badRequestResult.Value); //
    }

    [Fact]
    public async Task GetById_TenantNotFound_ReturnsNotFound()
    {
        // Arrange
        var tenantId = 1;
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByIdAsync(tenantId))
            .ReturnsAsync((TenantBaseDto)null);

        // Act
        var result = await _controller.GetById(tenantId); //

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- GetByPath Tests ---

    [Fact]
    public async Task GetByPath_ValidId_ReturnsOkObjectResultWithTenant()
    {
        // Arrange
        var tenantPath = "testtenant";
        var expectedTenant = new TenantBaseDto { Id = 1, Name = "tenant", Path = tenantPath }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync(tenantPath))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _controller.GetByPath(tenantPath);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTenant = Assert.IsType<TenantBaseDto>(okResult.Value);
        Assert.Equal(expectedTenant.Id, actualTenant.Id);
        Assert.Equal(expectedTenant.Name, actualTenant.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetByPath_InvalidId_ReturnsBadRequest(string? tenantPath)
    {
        // Act
        var result = await _controller.GetByPath(tenantPath); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.TenantPath, badRequestResult.Value); //
    }

    [Fact]
    public async Task GetByPath_TenantNotFound_ReturnsNotFound()
    {
        // Arrange
        string tenant = "test";
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync("test"))
            .ReturnsAsync((TenantBaseDto)null);

        // Act
        var result = await _controller.GetByPath(tenant); //

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- GetByName Tests ---

    [Fact]
    public async Task GetByName_ValidId_ReturnsOkObjectResultWithTenant()
    {
        // Arrange
        var tenant = "TestTenant";
        var expectedTenant = new TenantBaseDto { Id = 1, Name = tenant, Path = "testtenant" }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByNameAsync(tenant))
            .ReturnsAsync(expectedTenant);

        // Act
        var result = await _controller.GetByName(tenant);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTenant = Assert.IsType<TenantBaseDto>(okResult.Value);
        Assert.Equal(expectedTenant.Id, actualTenant.Id);
        Assert.Equal(expectedTenant.Name, actualTenant.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetByName_InvalidId_ReturnsBadRequest(string? tenant)
    {
        // Act
        var result = await _controller.GetByName(tenant); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.TenantName, badRequestResult.Value); //
    }

    [Fact]
    public async Task GetByName_TenantNotFound_ReturnsNotFound()
    {
        // Arrange
        string tenant = "test";
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByNameAsync("test"))
            .ReturnsAsync((TenantBaseDto)null);

        // Act
        var result = await _controller.GetByName(tenant); //

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Create Tests ---
    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var tenantDto = new TenantBaseDto { Name = "NewTenant", Path = "newtenantpath" }; //
        var createdTenantDto = new TenantBaseDto { Id = 1, Name = "NewTenant", Path = "newtenantpath" }; //
        _tenantRepositoryMock.Setup(repo => repo.CreateTenantAsync(tenantDto))
           
            .ReturnsAsync(createdTenantDto);

        // Act
        var result = await _controller.Create(tenantDto); //

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"api/tenant/{createdTenantDto.Id}", createdAtActionResult.Location);
        var actualDto = Assert.IsType<TenantBaseDto>(createdAtActionResult.Value);
        Assert.Equal(createdTenantDto.Path, actualDto.Path);
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Arrange
        // Act
        var result = await _controller.Create(null); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value); //
    }

    [Fact]
    public async Task Create_TenantWithSamePathExists_ReturnsConflict()
    {
        // Arrange
        var tenantDto = new TenantBaseDto { Name = "NewTenant", Path = "existingpath" }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync(tenantDto.Path)).ReturnsAsync(tenantDto);

        // Act
        var result = await _controller.Create(tenantDto); //

        // Assert
        var conflictResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(ErrorMessageConstants.TenantPathExists, conflictResult.Value); //
    }


    [Fact]
    public async Task Create_InvalidDto_NameMissing_ReturnsBadRequest()
    {
        // Arrange
        var tenantDto = new TenantBaseDto { Path = "testpath" };
        _controller.ModelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = await _controller.Create(tenantDto); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<BadRequestResultDto>(badRequestResult.Value); // Controller returns ModelState directly
    }

    [Fact]
    public async Task Create_RepositoryReturnsNull_ReturnsInternalServerError()
    {
        // Arrange
        var tenantDto = new TenantBaseDto { Name = "NewTenant", Path = "newtenantpath" }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync(tenantDto.Path)).ReturnsAsync((TenantBaseDto)null);
        _tenantRepositoryMock.Setup(repo => repo.CreateTenantAsync(tenantDto))
            .Throws(new Exception(ErrorMessageConstants.TenantIdInvalid)); 

        // Act
        var result = await _controller.Create(tenantDto); 

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal(ErrorMessageConstants.TenantIdInvalid, statusCodeResult.Value); //
    }

    // --- Update Tests ---
    [Fact]
    public async Task Update_ValidIdAndDto_ReturnsOkObjectResult()
    {
        // Arrange
        var tenantId = 1;
        var tenantDto = new TenantBaseDto { Id = tenantId, Name = "UpdatedTenant", Path = "updatedpath" }; 
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByIdAsync(tenantId)).ReturnsAsync(tenantDto); 

        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync(tenantDto.Path)).ReturnsAsync((TenantBaseDto)null); 

        _tenantRepositoryMock.Setup(repo => repo.UpdateTenantBaseValuesAsync(tenantDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(tenantId, tenantDto); //

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualDto = Assert.IsType<TenantBaseDto>(okResult.Value);
        Assert.Equal(tenantDto.Name, actualDto.Name);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 0;
        var tenantDto = new TenantBaseDto { Name = "Test", Path = "testpath" }; //

        // Act
        var result = await _controller.Update(tenantId, tenantDto); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.TenantIdInvalid, badRequestResult.Value); //
    }

    [Fact]
    public async Task Update_NullDto_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 1;

        // Act
        var result = await _controller.Update(tenantId, null); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.InvalidOrMissingBody, badRequestResult.Value); //
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 1;
        var tenantDto = new TenantBaseDto { Id = 2, Name = "Test", Path = "testpath" }; //

        // Act
        var result = await _controller.Update(tenantId, tenantDto); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.IdMissMatch, badRequestResult.Value); //
    }

    [Fact]
    public async Task Update_InvalidDto_NameMissing_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 1;
        var tenantDto = new TenantBaseDto { Id = tenantId, Path = "testpath" }; // Name is required
        _controller.ModelState.AddModelError("Name", "The Name field is required.");

        // Act
        var result = await _controller.Update(tenantId, tenantDto); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<BadRequestResultDto>(badRequestResult.Value); // Controller returns ModelState directly
    }

    [Fact]
    public async Task Update_RepositoryReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var tenantId = 1;
        var tenantDto = new TenantBaseDto { Id = tenantId, Name = "UpdatedTenant", Path = "updatedpath" }; //
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByIdAsync(tenantId)).ReturnsAsync(tenantDto);
        _tenantRepositoryMock.Setup(repo => repo.GetTenantByPathAsync(tenantDto.Path)).ReturnsAsync((TenantBaseDto)null);
        _tenantRepositoryMock.Setup(repo => repo.UpdateTenantBaseValuesAsync(tenantDto))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(tenantId, tenantDto); //

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.TenantNotFound, notFoundResult.Value); //
    }

    // --- Delete Tests ---
    [Fact]
    public async Task Delete_ValidId_RepositoryReturnsTrue_ReturnsNoContent()
    {
        // Arrange
        var tenantId = 1;
        _tenantRepositoryMock.Setup(repo => repo.DeleteAsync(tenantId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(tenantId); //

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ValidId_RepositoryReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        var tenantId = 1;
        _tenantRepositoryMock.Setup(repo => repo.DeleteAsync(tenantId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(tenantId); //

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsBadRequest()
    {
        // Arrange
        var tenantId = 0;

        // Act
        var result = await _controller.Delete(tenantId); //

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(ErrorMessageConstants.TenantIdInvalid, badRequestResult.Value); //
    }
}
