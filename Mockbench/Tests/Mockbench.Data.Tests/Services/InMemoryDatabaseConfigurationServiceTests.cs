using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Data.Services;
using Mockbench.Shared.Models.Configuration;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Tests.Service
{
    public class InMemoryDatabaseConfigurationServiceTests
    {
        private readonly DbContextOptions<MockbenchDbContext> _dbContextOptions;
        private readonly IOptions<DeploymentConfiguration> _deploymentConfigurationOptions;

        public InMemoryDatabaseConfigurationServiceTests()
        {
            // Configure the DbContext to use a new in-memory database for each test run.
            _dbContextOptions = new DbContextOptionsBuilder<MockbenchDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create a default DeploymentConfiguration and wrap it in IOptions for dependency injection.
            var deploymentConfiguration = new DeploymentConfiguration();
            _deploymentConfigurationOptions = Options.Create(deploymentConfiguration);
        }

        [Fact]
        public async Task GetPendingMigrationsAsync_ShouldReturnEmptyList()
        {
            // Arrange
            // Pass both required dependencies to the constructor.
            await using var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions);
            var service = new InMemoryDatabaseConfigurationService(context);

            // Act
            var result = await service.GetPendingMigrationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task DoesConnectionStringWorkAsync_ShouldAlwaysSucceed()
        {
            // Arrange
            await using var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions);
            var service = new InMemoryDatabaseConfigurationService(context);

            // Act
            var result = await service.DoesConnectionStringWorkAsync("any-string");

            // Assert
            Assert.Equal(ConnectionStringStatus.Success, result);
        }

        [Fact]
        public async Task ApplyMigrationsAsync_ShouldCallSeedConfiguration()
        {
            // Arrange
            // Use our test-specific DbContext, passing both dependencies.
            await using var testableContext = new TestableMockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions);
            var service = new InMemoryDatabaseConfigurationService(testableContext);

            // Act
            await service.ApplyMigrationsAsync(string.Empty);

            // Assert
            Assert.True(testableContext.SeedConfigurationAsyncCalled);
        }

        [Fact]
        public async Task TestConnectionStringWorkAsync_ShouldReturnSuccessWithNoMigrations()
        {
            // Arrange
            await using var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions);
            var service = new InMemoryDatabaseConfigurationService(context);

            // Act
            var result = await service.TestConnectionStringWorkAsync("any-string");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ConnectionStringStatus.Success, result.ConnectionStringStatus);
            Assert.NotNull(result.PendingMigrations);
            Assert.Empty(result.PendingMigrations);
            Assert.Equal("In-memory provider does not use real connection strings.", result.Message);
        }

        [Fact]
        public void GetAllMigrations_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions);
            var service = new InMemoryDatabaseConfigurationService(context);

            // Act
            var result = service.GetAllMigrations();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}