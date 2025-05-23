using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Tests.Contexts
{
    // Note: This class now implements IDisposable to correctly manage the SQLite connection
    public class MockbenchDbContextTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<MockbenchDbContext> _dbContextOptions;
        private readonly IOptions<DeploymentConfiguration> _deploymentConfigurationOptions;

        public MockbenchDbContextTests()
        {
            // 1. Create and open a connection to an in-memory SQLite database
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // 2. Configure the DbContext to use this SQLite connection
            _dbContextOptions = new DbContextOptionsBuilder<MockbenchDbContext>()
                .UseSqlite(_connection)
                .Options;

            var deploymentConfig = new DeploymentConfiguration { DefaultTheme = "Cosmic", DefaultUIMode = UIMode.SingleMicroservice };
            _deploymentConfigurationOptions = Options.Create(deploymentConfig);

            // 3. Create the database schema based on your model
            using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new MockbenchDbContext(_dbContextOptions, null));
        }

        [Fact]
        public async Task SeedConfigurationAsync_WhenSettingsExist_DoesNotAddMoreSettings()
        {
            // Arrange
            // Add initial settings to the database
            await using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                context.Settings.Add(new Settings { Id = 1, PreferredTheme = "Initial", UIMode = UIMode.Full });
                await context.SaveChangesAsync();
            }

            // Act
            await using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                await context.SeedConfigurationAsync();
            }

            // Assert
            await using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                Assert.Equal(1, await context.Settings.CountAsync());
                var setting = await context.Settings.FirstAsync();
                Assert.Equal("Initial", setting.PreferredTheme);
            }
        }

        [Fact]
        public async Task SeedConfigurationAsync_WhenNoSettingsExist_AddsDefaultSettings()
        {
            // Arrange - The database is created fresh for this test run and has no settings.

            // Act
            await using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                await context.SeedConfigurationAsync();
            }

            // Assert
            await using (var context = new MockbenchDbContext(_dbContextOptions, _deploymentConfigurationOptions))
            {
                Assert.Equal(1, await context.Settings.CountAsync());
                var setting = await context.Settings.FirstAsync();
                Assert.Equal("Cosmic", setting.PreferredTheme);
                Assert.Equal(UIMode.SingleMicroservice, setting.UIMode);
            }
        }

        /// <summary>
        /// This method is called by the xUnit test runner after each test has run.
        /// It ensures the database connection is closed, and the in-memory database is destroyed.
        /// </summary>
        public void Dispose()
        {
            _connection.Close();
        }
    }
}