using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Contexts
{
    public class MockbenchDbContext : DbContext
    {
        private readonly DeploymentConfiguration _deploymentConfiguration;

        public MockbenchDbContext(DbContextOptions options, IOptions<DeploymentConfiguration> deploymentConfigurationOptions) : base(options)
        {
            _deploymentConfiguration = deploymentConfigurationOptions?.Value ?? throw new ArgumentNullException(nameof(_deploymentConfiguration));
        }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<Models.Environment> Environments { get; set; }

        public DbSet<TenantVariable> TenantVariables { get; set; }

        public DbSet<EnvironmentVariable> EnvironmentVariables { get; set; }

        public DbSet<Endpoint> Endpoints { get; set; }

        public DbSet<MockResponse> MockResponses { get; set; }
        
        public DbSet<Microservice> Microservices { get; set; }
        
        public DbSet<QueryParameter> QueryParameters { get; set; }

        public DbSet<ServiceHeader> ServiceHeaders { get; set; }
        
        public DbSet<EndpointHeader> EndpointHeaders { get; set; }
        
        public DbSet<ResponseHeader> ResponseHeaders { get; set; }

        public DbSet<Settings> Settings { get; set; }
        
        public async Task SeedConfigurationAsync()
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
