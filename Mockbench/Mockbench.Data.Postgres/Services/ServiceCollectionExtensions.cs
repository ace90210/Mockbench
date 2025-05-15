using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Data.Postgres.Contexts;
using Mockbench.Data.PostgresProvider.Services;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Postgres.Services;

public static class ServiceCollectionExtensions
{
   public static IServiceCollection AddPostgresServices(this IServiceCollection services, DeploymentConfiguration deploymentConfiguration)
   {
        var migrationAssembly = typeof(ServiceCollectionExtensions).Assembly.GetName().Name;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString 
                ?? deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly)));

        services.AddDbContext<PostgresMockbenchDbContext>(options =>
            options.UseNpgsql(
                deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly)));

        services.AddScoped<MockbenchDbContext, PostgresMockbenchDbContext>();
        services.AddScoped<IDatabaseConfigurationService, PostgresDatabaseConfigurationService>();

        return services;
    }
}
