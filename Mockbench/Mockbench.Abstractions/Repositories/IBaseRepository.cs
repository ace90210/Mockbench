using Mockbench.Shared.Models.Endpoint;

namespace Mockbench.Abstractions.Repositories
{
    public interface IBaseRepository
    {
        Task<(bool, MatchingEndpoints?)> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(MatchingEndpoints matchingEndpoints);
        
        Task<(bool, MatchingEndpoints?)> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(string? tenantPath, string? environmentPath, string? microservicePath);

        bool ValidateTenantEnvironmentMicroserviceIfNotExists(string? tenantPath, string? environmentPath, string? microservicePath);
    }
}