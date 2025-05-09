using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Sqlite.Services
{
    public class SqliteDatabaseConnectionService : IDatabaseConfigurationService
    {
        private readonly MockbenchMainContext _mainContext;
        private readonly ILogger<SqliteDatabaseConnectionService> _logger;

        public SqliteDatabaseConnectionService(MockbenchMainContext mainContext, ILogger<SqliteDatabaseConnectionService> logger)
        {
            _mainContext = mainContext ?? throw new ArgumentNullException(nameof(mainContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                return ConnectionStringStatus.ConnectNoDatabase;
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

        public async Task ApplyMigrationsAsync(string connectionString)
        {
            var sqliteFilePath = GetFilePathFromConnectionString(connectionString);

            if (!string.IsNullOrWhiteSpace(sqliteFilePath))
            {
                // check directory exists
                if (!Directory.Exists(sqliteFilePath))
                {
                    Directory.CreateDirectory(sqliteFilePath);
                }
            }
            
            await _mainContext.Database.MigrateAsync();

            await _mainContext.SeedConfigurationAsync();
        }

        private string GetFilePathFromConnectionString(string connectionString)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            string filePath = ((string)builder["Data Source"]).Trim();

            return Path.GetDirectoryName(filePath);
        }

        public async Task<ConnectionStringTestResult> TestConnectionStringWorkAsync(string connectionString)
        {
                var result = new ConnectionStringTestResult();
            try
            {

                result.ConnectionStringStatus = ConnectionStringStatus.Success;
                result.PendingMigrations = await _mainContext.Database.GetPendingMigrationsAsync();
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"test failed");
                result.ConnectionStringStatus = ConnectionStringStatus.Failed;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public IEnumerable<string> GetAllMigrations()
        {
            return _mainContext.Database.GetMigrations();
        }
    }
}
