using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Postgres.Contexts;
using Mockbench.Shared.Models.Utility;
using Npgsql; 
using System.Text;

namespace Mockbench.Data.PostgresProvider.Services;

public class PostgresDatabaseConfigurationService : IDatabaseConfigurationService
{
    private readonly PostgresMockbenchDbContext _mainContext; // Inject the Postgres-specific context

    public PostgresDatabaseConfigurationService(PostgresMockbenchDbContext mainContext)
    {
        _mainContext = mainContext;
    }

    public async Task<IEnumerable<string>> GetPendingMigrationsAsync()
    {
        return await _mainContext.Database.GetPendingMigrationsAsync();
    }

    public IEnumerable<string> GetAllMigrations()
    {
        return _mainContext.Database.GetMigrations();
    }

    public async Task ApplyMigrationsAsync(string _) // The string parameter seems unused, same as original
    {
        await _mainContext.Database.MigrateAsync();
        await _mainContext.SeedConfigurationAsync(); // Assuming SeedConfigurationAsync is on the base or Postgres context
    }

    private async Task<bool> CanConnectWithTimeoutAsync(string connectionString, CancellationToken cancellationToken)
    {
        // Temporarily change the context's connection string for CanConnectAsync
        var originalConnectionString = _mainContext.Database.GetDbConnection().ConnectionString;
        _mainContext.Database.GetDbConnection().ConnectionString = connectionString;
        try
        {
            return await _mainContext.Database.CanConnectAsync(cancellationToken);
        }
        finally
        {
            // Restore the original connection string if the context is pooled or reused
            _mainContext.Database.GetDbConnection().ConnectionString = originalConnectionString;
        }
    }

    /// <summary>
    /// Modifies the Npgsql connection string to target a different database (e.g., a maintenance database)
    /// or removes the database part if replacementDb is null (though connecting without a DB is unusual for Npgsql).
    /// </summary>
    private string GetModifiedNpgsqlConnectionString(string connectionString, string targetDatabase = "postgres")
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                // TODO Pooling = false // Disable pooling for transient test connections if necessary
            };

            builder.Database = targetDatabase;
            return builder.ToString();
        }
        catch (ArgumentException) 
        {
            return null; // TODO handle better
        }
    }


    public async Task<ConnectionStringStatus> DoesConnectionStringWorkAsync(string connectionString)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (await CanConnectWithTimeoutAsync(connectionString, cts.Token))
            {
                return ConnectionStringStatus.Success;
            }

            // If direct connection fails, try connecting to the 'postgres' maintenance database
            // to see if the server is up but the specific application database is missing/inaccessible.
            string maintenanceDbConnectionString = GetModifiedNpgsqlConnectionString(connectionString, "postgres");
            if (maintenanceDbConnectionString == null) return ConnectionStringStatus.Failed; // Invalid original format

            using var ctsMaintenance = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (await CanConnectWithTimeoutAsync(maintenanceDbConnectionString, ctsMaintenance.Token))
            {
                return ConnectionStringStatus.ConnectNoDatabase;
            }

            return ConnectionStringStatus.Failed;
        }
        catch (Exception)
        {
            return ConnectionStringStatus.Failed;
        }
    }

    public async Task<ConnectionStringTestResult> TestConnectionStringWorkAsync(string connectionString)
    {
        var result = new ConnectionStringTestResult();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Longer timeout for comprehensive test

            // Attempt to connect with the provided connection string
            if (await CanConnectWithTimeoutAsync(connectionString, cts.Token))
            {
                result.ConnectionStringStatus = ConnectionStringStatus.Success;
                // Restore connection string for subsequent EF operations
                _mainContext.Database.GetDbConnection().ConnectionString = connectionString;
                result.PendingMigrations = await _mainContext.Database.GetPendingMigrationsAsync();
                return result;
            }

            // If direct connection fails, try connecting to the 'postgres' maintenance database
            string maintenanceDbConnectionString = GetModifiedNpgsqlConnectionString(connectionString, "postgres");
            if (maintenanceDbConnectionString == null) // Invalid original format
            {
                result.ConnectionStringStatus = ConnectionStringStatus.Failed;
                result.Message = "Original connection string format is invalid.";
                // Attempt to get defined migrations if context is somehow configured (might fail)
                try { result.PendingMigrations = _mainContext.Database.GetMigrations(); } catch { /* ignored */ }
                return result;
            }

            using var ctsMaintenance = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (await CanConnectWithTimeoutAsync(maintenanceDbConnectionString, ctsMaintenance.Token))
            {
                result.ConnectionStringStatus = ConnectionStringStatus.ConnectNoDatabase;
                result.Message = $"Successfully connected to the PostgreSQL server (used '{new NpgsqlConnectionStringBuilder(maintenanceDbConnectionString).Database}' database for test), but the specified database '{new NpgsqlConnectionStringBuilder(connectionString).Database}' may not exist or is not accessible.";
                // In this state, all migrations for the application are considered pending
                // because the application database itself is not confirmed.
                _mainContext.Database.GetDbConnection().ConnectionString = connectionString; // set back for GetMigrations if needed
                try { result.PendingMigrations = _mainContext.Database.GetMigrations(); } catch { /* ignored */ }
                return result;
            }

            result.ConnectionStringStatus = ConnectionStringStatus.Failed;
            result.Message = "Could not connect to the database server or the specified database.";
            try { result.PendingMigrations = _mainContext.Database.GetMigrations(); } catch { /* ignored */ }
            return result;
        }
        catch (Exception ex)
        {
            result.ConnectionStringStatus = ConnectionStringStatus.Failed;
            result.Message = GetFullExceptionMessage(ex);
            try { result.PendingMigrations = _mainContext.Database.GetMigrations(); } catch { /* ignored */ }
            return result;
        }
    }

    private string GetFullExceptionMessage(Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Exception occurred while testing database connection:");

        Exception currentEx = ex;
        while (currentEx != null)
        {
            sb.AppendLine($"Type: {currentEx.GetType().FullName}");
            sb.AppendLine($"Message: {currentEx.Message}");
            sb.AppendLine($"StackTrace: {currentEx.StackTrace}");
            sb.AppendLine("---");
            currentEx = currentEx.InnerException;
        }
        return sb.ToString();
    }
}