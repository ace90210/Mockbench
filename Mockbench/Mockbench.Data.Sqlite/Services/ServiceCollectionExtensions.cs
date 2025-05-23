using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Data.Sqlite.Contexts;
using Mockbench.Shared.Models.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Mockbench.Data.Sqlite.Services;

public static class ServiceCollectionExtensions
{

    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddSqliteServices(this IServiceCollection services, IWebHostEnvironment environment, DeploymentConfiguration deploymentConfiguration)
    {
        var migrationAssembly = typeof(ServiceCollectionExtensions).Assembly.GetName().Name;

        var authenticationConnectionString = deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString ?? deploymentConfiguration.DatabaseConfig.MainConnectionString;

        EnsureDbFolderCreated(environment, authenticationConnectionString);
        EnsureDbFolderCreated(environment, deploymentConfiguration.DatabaseConfig.MainConnectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString
                ?? deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly))
            );

        services.AddDbContext<SqliteMockbenchDbContext>(options =>
            options.UseSqlite(
                deploymentConfiguration.DatabaseConfig.MainConnectionString,
                b => b.MigrationsAssembly(migrationAssembly)));

        services.AddScoped<MockbenchDbContext, SqliteMockbenchDbContext>();
        services.AddScoped<IDatabaseConfigurationService, SqliteDatabaseConfigurationService>();

        return services;
    }

    private static void EnsureDbFolderCreated(IWebHostEnvironment webHostEnvironment, string connectionString)
    {
        try
        {
            // Extract path from Data Source= path
            var dataSourcePrefix = "Data Source=";
            var startIndex = connectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                var path = connectionString.Substring(startIndex + dataSourcePrefix.Length).Trim();
                var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(webHostEnvironment.ContentRootPath, path);
                var directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }
        catch (Exception ex)
        {
            // Optionally log or handle exception (e.g. log to Debug output)
            Console.WriteLine($"[SQLite Init] Failed to create directory for SQLite DB: {ex.Message}");
        }
    }
}
