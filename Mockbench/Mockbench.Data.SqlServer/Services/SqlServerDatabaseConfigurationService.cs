using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Shared.Models.Utility;
using System.Text;

namespace Mockbench.Data.SqlServer.Services
{
    public class SqlServerDatabaseConfigurationService : IDatabaseConfigurationService
    {
        private readonly MockbenchMainContext _mainContext;

        public SqlServerDatabaseConfigurationService(MockbenchMainContext mainContext)
        {
            _mainContext = mainContext;
        }

        public async Task<IEnumerable<string>> GetPendingMigrationsAsync()
        {
            return await _mainContext.Database.GetPendingMigrationsAsync();
        }

        public async Task<ConnectionStringStatus> DoesConnectionStringWorkAsync(string connectionString)
        {
            _mainContext.Database.GetDbConnection().ConnectionString = connectionString;
            try
            {
                if (await TimeoutConnect())
                    return ConnectionStringStatus.Success;
               
                _mainContext.Database.GetDbConnection().ConnectionString = RemoveDatabaseFromSqlConnectionString(connectionString);

                if (await TimeoutConnect())
                {
                    _mainContext.Database.GetDbConnection().ConnectionString = connectionString;
                    return ConnectionStringStatus.ConnectNoDatabase;
                }
                
                return ConnectionStringStatus.Failed;
            }
            catch (Exception)
            {
                return ConnectionStringStatus.Failed;
            }
        }

        private async Task<bool> TimeoutConnect()
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                return await _mainContext.Database.CanConnectAsync(cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string RemoveDatabaseFromSqlConnectionString(string connectionString)
        {
            string output = connectionString;
            int indexOfDatabase = output.IndexOf(";Database=", StringComparison.Ordinal);
            if (indexOfDatabase > -1)
            {
                int indexOfNextSemicolon = output.IndexOf(';', indexOfDatabase + 1);
                output = output.Remove(indexOfDatabase + 1, indexOfNextSemicolon - indexOfDatabase);
            }
            return output;
        }

        public async Task ApplyMigrationsAsync(string _)
        {
            await _mainContext.Database.MigrateAsync();

            await _mainContext.SeedConfigurationAsync();
        }

        public async Task<ConnectionStringTestResult> TestConnectionStringWorkAsync(string connectionString)
        {
            var result = new ConnectionStringTestResult();
            var connection = _mainContext.Database.GetDbConnection();
            connection.ConnectionString = connectionString;

            try
            {
                if (await _mainContext.Database.CanConnectAsync())
                {
                    result.ConnectionStringStatus = ConnectionStringStatus.Success;
                    result.PendingMigrations = await _mainContext.Database.GetPendingMigrationsAsync();
                    return result;
                }

                // Attempt to connect without a specific database
                connection.ConnectionString = RemoveDatabaseFromSqlConnectionString(connectionString);

                if (await _mainContext.Database.CanConnectAsync())
                {
                    connection.ConnectionString = connectionString;
                    result.ConnectionStringStatus = ConnectionStringStatus.ConnectNoDatabase;
                    result.Message = "Able to connect, but no database found.";
                    result.PendingMigrations = _mainContext.Database.GetMigrations();
                    return result;
                }

                return await TestConnectionStringFallBackAsync(connectionString);
            }
            catch (Exception ex)
            {
                result.ConnectionStringStatus = ConnectionStringStatus.Failed;
                result.Message = GetFullExceptionMessage(ex);
                result.PendingMigrations = _mainContext.Database.GetMigrations();
                return result;
            }
        }

        public IEnumerable<string> GetAllMigrations()
        {
            return _mainContext.Database.GetMigrations();
        }

        private async Task<ConnectionStringTestResult> TestConnectionStringFallBackAsync(string connectionString)
        {
            var result = new ConnectionStringTestResult();
            var connection = _mainContext.Database.GetDbConnection();
            connection.ConnectionString = connectionString;

            try
            {
                // Try to open the connection manually
                await connection.OpenAsync();
                result.ConnectionStringStatus = ConnectionStringStatus.Success;
                result.PendingMigrations = await _mainContext.Database.GetPendingMigrationsAsync();

                // Close the connection after the test
                await connection.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                // Log the full exception message including inner exceptions
                result.ConnectionStringStatus = ConnectionStringStatus.Failed;
                result.Message = GetFullExceptionMessage(ex);
                result.PendingMigrations = _mainContext.Database.GetMigrations();
                return result;
            }
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Exception occurred while testing database connection:");
            sb.AppendLine(ex.Message);

            var inner = ex.InnerException;
            while (inner is not null)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }
    }
}
