using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Mockbench.Data.Contexts
{
    public class MockbenchMainContext : DbContext
    {
        private readonly DeploymentConfiguration _deploymentConfiguration;

        public MockbenchMainContext(DbContextOptions options, IOptions<DeploymentConfiguration> deploymentConfigurationOptions) : base(options)
        {
            _deploymentConfiguration = deploymentConfigurationOptions?.Value;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>()
                .HasIndex(rt => rt.Path)
                .IsUnique();

            // Configuration for Endpoints
            modelBuilder.Entity<Endpoint>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configuration for ServiceHeaders
            modelBuilder.Entity<ServiceHeader>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

            // Configuration for Environments
            modelBuilder.Entity<Models.Environment>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

            // Configuration for MockResponses
            modelBuilder.Entity<MockResponse>(entity =>
            {
                entity.Property(e => e.Priority)
                    .HasColumnType("int")
                    .HasDefaultValue(100);

                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configuration for Microservices
            modelBuilder.Entity<Microservice>(entity =>
            {
                entity.Property(e => e.PassThroughTenant)
                    .HasColumnType("bit"); 

                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

            // seed if not exists
            modelBuilder.Entity<Settings>();
        }

        

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
