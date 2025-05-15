using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Data.Contexts;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Data.Services
{
    public class InMemoryDatabaseConfigurationService : IDatabaseConfigurationService
    {
        private readonly MockbenchDbContext _mainContext;

        public InMemoryDatabaseConfigurationService(MockbenchDbContext mainContext)
        {
            _mainContext = mainContext;
        }

        public Task<IEnumerable<string>> GetPendingMigrationsAsync()
        {
            // In-memory DB doesn't track real migrations
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public Task<ConnectionStringStatus> DoesConnectionStringWorkAsync(string connectionString)
        {
            // Always succeeds for in-memory
            return Task.FromResult(ConnectionStringStatus.Success);
        }

        public Task ApplyMigrationsAsync(string _)
        {
            // Apply seeding if needed
            return _mainContext.SeedConfigurationAsync();
        }

        public Task<ConnectionStringTestResult> TestConnectionStringWorkAsync(string connectionString)
        {
            // Always succeed and return no migrations
            return Task.FromResult(new ConnectionStringTestResult
            {
                ConnectionStringStatus = ConnectionStringStatus.Success,
                PendingMigrations = Array.Empty<string>(),
                Message = "In-memory provider does not use real connection strings."
            });
        }

        public IEnumerable<string> GetAllMigrations()
        {
            // No migrations in memory
            return Enumerable.Empty<string>();
        }
    }
}
