using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Data.Models;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Sqlite.Contexts;

public class SqliteMockbenchDbContext : MockbenchDbContext
{
    public override bool IsRelationalDatabase => true;
    public SqliteMockbenchDbContext(
        DbContextOptions<SqliteMockbenchDbContext> options, 
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
                .HasColumnType("INTEGER")
                .HasDefaultValue(1);    

            entity.Property(e => e.CreatedUtc)
                .HasColumnType("TEXT") 
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ServiceHeader>(entity =>
        {
            entity.Property(e => e.Enabled)
                .HasColumnType("INTEGER")
                .HasDefaultValue(1);
        });

        modelBuilder.Entity<Models.Environment>(entity =>
        {
            entity.Property(e => e.Enabled)
                .HasColumnType("INTEGER")
                .HasDefaultValue(1);
        });

        modelBuilder.Entity<MockResponse>(entity =>
        {
            entity.Property(e => e.Priority)
                  .HasColumnType("INTEGER"); 

            entity.Property(e => e.Enabled)
                .HasColumnType("INTEGER")
                .HasDefaultValue(1);

            entity.Property(e => e.CreatedUtc)
                .HasColumnType("TEXT")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Microservice>(entity =>
        {
            entity.Property(e => e.PassThroughTenant)
                .HasColumnType("INTEGER");

            entity.Property(e => e.Enabled)
                .HasColumnType("INTEGER")
                .HasDefaultValue(1);
        });

        // Note: For SQLite, some global model conventions might be useful if you're targeting older
        // versions or need specific behaviors, but EF Core's defaults are generally good.
        // For example, to ensure DATETIME columns are stored as TEXT:
        // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        // {
        //     foreach (var property in entityType.GetProperties())
        //     {
        //         if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        //         {
        //             property.SetColumnType("TEXT");
        //         }
        //     }
        // }
        // However, rely on EF Core's default mappings first and override per-property as needed.
    }
}

