using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.Repositories;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Data.Repositories;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Tests.Repositories;

public class CommonRepositoryTests
{
    private MockbenchDbContext _context;
    private ICommonRepository _repository;
    private IOptions<DeploymentConfiguration> _deploymentOptions;

    public CommonRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MockbenchDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var deploymentConfig = new DeploymentConfiguration { DatabaseConfig = new DatabaseConfig { Provider = DatabaseProvider.InMemory } };

        _deploymentOptions = Options.Create(deploymentConfig);

        _context = new MockbenchDbContext(options, _deploymentOptions);


        _repository = new CommonRepository(_context, _deploymentOptions);
    }


    private void SeedDatabase()
    {
        var tenant = new Tenant { Id = 1, Name = "Test Tenant", Path = "tenant", SimulateTime = DateTime.UtcNow.AddHours(-1) };
        var environment = new Models.Environment { ID = 1, Name = "Test Environment", Path = "env", SimulateTime = DateTime.UtcNow.AddHours(-2) };
        var microservice = new Microservice { Id = 1, Name = "Test Microservice", Path = "service", SimulateTime = DateTime.UtcNow.AddHours(-3) };
        var endpoint = new Endpoint { Id = 1, FromUrl = "/test", MicroserviceId = 1, SimulateTime = DateTime.UtcNow.AddHours(-4) };
        var mockResponse = new MockResponse { Id = 1, EndpointId = 1, CreatedUtc = DateTime.UtcNow };

        _context.Tenants.Add(tenant);
        _context.Environments.Add(environment);
        _context.Microservices.Add(microservice);
        _context.Endpoints.Add(endpoint);
        _context.MockResponses.Add(mockResponse);
        _context.SaveChanges();
    }

    // Set Simulation Time Tests
    [Fact]
    public async Task SetSimulateTimeOnRequest_WhenEndpointExists_ReturnsTrue()
    {
        // Arrange
        SeedDatabase();
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnRequest(newTime, 1);

        // Assert
        Assert.True(result);
        var endpoint = await _context.Endpoints.FindAsync(1);
        Assert.Equal(newTime, endpoint.SimulateTime);
    }

    [Fact]
    public async Task SetSimulateTimeOnRequest_WhenEndpointDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnRequest(newTime, 99);

        // Assert
        Assert.False(result);
    }

    // Get Times Tests
    [Fact]
    public async Task GetRequestTimes_WhenEndpointExists_ReturnsTimes()
    {
        // Arrange
        SeedDatabase();

        // Act
        var result = await _repository.GetRequestTimes(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AvailableTimes.Count);
        Assert.NotNull(result.CurrentTime);
    }

    [Fact]
    public async Task GetRequestTimes_WhenEndpointDoesNotExist_ReturnsEmptyDto()
    {
        // Act
        var result = await _repository.GetRequestTimes(99);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.AvailableTimes);
        Assert.Null(result.CurrentTime);
    }

    [Fact]
    public async Task GetMicroserviceTimes_WhenMicroserviceExists_ReturnsTimes()
    {
        // Arrange
        SeedDatabase();

        // Act
        var result = await _repository.GetMicroserviceTimes(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AvailableTimes.Count);
        Assert.NotNull(result.CurrentTime);
    }

    [Fact]
    public async Task GetEnvironmentTimes_WhenEnvironmentExists_ReturnsTimes()
    {
        // Arrange
        SeedDatabase();

        // Act
        var result = await _repository.GetEnvironmentTimes(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AvailableTimes.Count);
        Assert.NotNull(result.CurrentTime);
    }

    [Fact]
    public async Task GetTenantEnvironmentTimes_WhenTenantExists_ReturnsTimes()
    {
        // Arrange
        SeedDatabase();

        // Act
        var result = await _repository.GetTenanEnvironmentTimes(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AvailableTimes.Count);
        Assert.NotNull(result.CurrentTime);
    }


    // Import/Export Tests
    [Fact]
    public async Task ExportDatabaseToJson_WhenDataExists_ReturnsFullDatabaseDtoWithZeroedIds()
    {
        // Arrange
        SeedDatabase();

        // Act
        var result = await _repository.ExportDatabaseToJson();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("InMemory", result.DatabaseType);
        Assert.Equal(1, result.Tenants.Count());
        Assert.Equal(0, result.Tenants.First().Id);
        Assert.Equal(1, result.Environments.Count());
        Assert.Equal(0, result.Environments.First().Id);
        Assert.Equal(1, result.Microservices.Count());
        Assert.Equal(1, result.Microservices.First().Id);
    }

    [Fact]
    public async Task ImportDatabase_WithNewEntities_ImportsSuccessfully()
    {
        // Arrange
        var importDto = new FullDatabaseDto
        {
            Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "New Tenant", Path = "new-tenant" } },
            Environments = new List<EnvironmentDto> { new EnvironmentDto { Name = "New Env", Path = "new-env" } },
            Microservices = new List<MicroserviceDto> { new MicroserviceDto { Name = "New Service", Path = "new-service" } }
        };

        // Act
        var result = await _repository.ImportDatabase(importDto, false);

        // Assert
        Assert.True(result);
        Assert.Equal(1, _context.Tenants.Count());
        Assert.Equal(1, _context.Environments.Count());
        Assert.Equal(1, _context.Microservices.Count());
    }

    [Fact]
    public async Task ImportDatabase_WithDuplicateTenantAndSkipDuplicatesFalse_ThrowsException()
    {
        // Arrange
        SeedDatabase();
        var importDto = new FullDatabaseDto
        {
            Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "Test Tenant", Path = "tenant" } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.ImportDatabase(importDto, false));
    }

    [Fact]
    public async Task ImportDatabase_WithDuplicateTenantAndSkipDuplicatesTrue_DoesNotThrowException()
    {
        // Arrange
        SeedDatabase();
        var importDto = new FullDatabaseDto
        {
            Tenants = new List<TenantBaseDto> { new TenantBaseDto { Name = "Test Tenant", Path = "tenant" } }
        };

        // Act
        var result = await _repository.ImportDatabase(importDto, true);

        // Assert
        Assert.True(result);
        Assert.Equal(1, _context.Tenants.Count());
    }

    [Fact]
    public async Task SetSimulateTimeOnMicroservice_WhenMicroserviceExists_ReturnsTrue()
    {
        // Arrange
        SeedDatabase();
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnMicroservice(newTime, 1);

        // Assert
        Assert.True(result);
        var microservice = await _context.Microservices.FindAsync(1);
        Assert.Equal(newTime, microservice.SimulateTime);
    }

    [Fact]
    public async Task SetSimulateTimeOnMicroservice_WhenMicroserviceDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnMicroservice(newTime, 99);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetSimulateTimeOnEnvironment_WhenEnvironmentExists_ReturnsTrue()
    {
        // Arrange
        SeedDatabase();
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnEnvironment(newTime, 1);

        // Assert
        Assert.True(result);
        var environment = await _context.Environments.FindAsync(1);
        Assert.Equal(newTime, environment.SimulateTime);
    }

    [Fact]
    public async Task SetSimulateTimeOnEnvironment_WhenEnvironmentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnEnvironment(newTime, 99);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetSimulateTimeOnTenant_WhenTenantExists_ReturnsTrue()
    {
        // Arrange
        SeedDatabase();
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnTenant(newTime, 1);

        // Assert
        Assert.True(result);
        var tenant = await _context.Tenants.FindAsync(1);
        Assert.Equal(newTime, tenant.SimulateTime);
    }

    [Fact]
    public async Task SetSimulateTimeOnTenant_WhenTenantDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var newTime = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSimulateTimeOnTenant(newTime, 99);

        // Assert
        Assert.False(result);
    }
}
