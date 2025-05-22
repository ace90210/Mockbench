using Mockbench.Shared.Models.Endpoint;

namespace Mockbench.Abstractions.Repositories
{
    public interface IBaseRepository
    {
        Task<MatchingEndpoints?> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(MatchingEndpoints matchingEndpoints);
        
        Task<MatchingEndpoints?> CreateTenantEnvironmentMicroserviceIfNotExistsAsync(string? tenantPath, string? environmentPath, string? microservicePath);

        bool ValidateTenantEnvironmentMicroserviceIfNotExists(string? tenantPath, string? environmentPath, string? microservicePath);
    }
}