using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.SqlServer.Contexts
{
    public class SqlServerMockbenchDbContext : MockbenchDbContext
    {
        public SqlServerMockbenchDbContext(
            DbContextOptions<SqlServerMockbenchDbContext> options, 
            IOptions<DeploymentConfiguration> deploymentConfigurationOptions)
            : base(options, deploymentConfigurationOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {           
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Endpoint>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<ServiceHeader>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<Models.Environment>(entity => 
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<MockResponse>(entity =>
            {
                entity.Property(e => e.Priority)
                        .HasColumnType("int"); 

                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Microservice>(entity =>
            {
                entity.Property(e => e.PassThroughTenant)
                    .HasColumnType("bit");

                entity.Property(e => e.Enabled)
                    .HasColumnType("bit")
                    .HasDefaultValue(true);
            });

        }
    }
    
}
