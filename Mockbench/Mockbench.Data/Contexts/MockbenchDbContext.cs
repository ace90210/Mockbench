using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Mockbench.Data.Contexts
{
    public class MockbenchDbContext : DbContext
    {
        private readonly DeploymentConfiguration _deploymentConfiguration;

        public virtual bool IsRelationalDatabase => false;

        [ExcludeFromCodeCoverage]
        public MockbenchDbContext(DbContextOptions options, IOptions<DeploymentConfiguration> deploymentConfigurationOptions) : base(options)
        {
            _deploymentConfiguration = deploymentConfigurationOptions?.Value ?? throw new ArgumentNullException(nameof(_deploymentConfiguration));
        }

        [ExcludeFromCodeCoverage]
        public DbSet<Tenant> Tenants { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<Models.Environment> Environments { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<TenantVariable> TenantVariables { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<EnvironmentVariable> EnvironmentVariables { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<Endpoint> Endpoints { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<MockResponse> MockResponses { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<Microservice> Microservices { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<QueryParameter> QueryParameters { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<ServiceHeader> ServiceHeaders { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<EndpointHeader> EndpointHeaders { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<ResponseHeader> ResponseHeaders { get; set; }

        [ExcludeFromCodeCoverage]
        public DbSet<Settings> Settings { get; set; }
        
        public virtual async Task SeedConfigurationAsync()
        {
            // Check if the UserConfiguration table exists
            var pendingMigrations = await Database.GetPendingMigrationsAsync();

            // If table exists and has no rows, insert default configuration
            if (!pendingMigrations.Any() && !Settings.Any())
            {
                Settings.Add(new Settings
                    {
                    Id = 1,
                    PreferredTheme = _deploymentConfiguration.DefaultTheme,
                    UIMode = _deploymentConfiguration.DefaultUIMode
                });

                await SaveChangesAsync();
            }
        }
    }
}
