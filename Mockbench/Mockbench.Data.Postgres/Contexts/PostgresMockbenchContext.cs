using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Postgres.Contexts
{
    public class PostgresMockbenchContext : MockbenchDbContext
    {
        public PostgresMockbenchContext(
            DbContextOptions<PostgresMockbenchContext> options, 
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
                    .HasColumnType("boolean") 
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW()"); 
            });

            modelBuilder.Entity<ServiceHeader>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("boolean")
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<Models.Environment>(entity =>
            {
                entity.Property(e => e.Enabled)
                    .HasColumnType("boolean")
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<MockResponse>(entity =>
            {
                entity.Property(e => e.Priority)
                      .HasColumnType("integer"); 

                entity.Property(e => e.Enabled)
                    .HasColumnType("boolean")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Microservice>(entity =>
            {
                entity.Property(e => e.PassThroughTenant)
                    .HasColumnType("boolean");

                entity.Property(e => e.Enabled)
                    .HasColumnType("boolean")
                    .HasDefaultValue(true);
            });

        }
    }
    
}
