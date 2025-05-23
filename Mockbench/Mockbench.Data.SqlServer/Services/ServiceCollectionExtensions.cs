using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Data.SqlServer.Contexts;
using Mockbench.Shared.Models.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Mockbench.Data.SqlServer.Services;

public static class ServiceCollectionExtensions
{
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddSqlServerServices(this IServiceCollection services, DeploymentConfiguration deploymentConfiguration)
   {
        var migrationAssembly = typeof(ServiceCollectionExtensions).Assembly.GetName().Name;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString 
                ?? deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly)));

        services.AddDbContext<SqlServerMockbenchDbContext>(options =>
            options.UseSqlServer(
                deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly)));

        services.AddScoped<MockbenchDbContext, SqlServerMockbenchDbContext>();
        services.AddScoped<IDatabaseConfigurationService, SqlServerDatabaseConfigurationService>();

        return services;
    }
}
