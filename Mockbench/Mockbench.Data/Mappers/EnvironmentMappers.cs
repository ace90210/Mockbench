using Mockbench.Shared.Models.Environment;

namespace Mockbench.Data.Mappers
{
    public static class EnvironmentMappers
    {
        #region Basic Environment Mappers
        public static BasicEnvironmentDto ToBasicEnvironmentDto(this Models.Environment environment)
        {
            return environment == null ? null : new BasicEnvironmentDto()
            {
                Id = environment.ID,
                Enabled = environment.Enabled,
                DefaultHealthCheckUrl = environment.DefaultHealthCheckUrl,
                Microservices = environment.Microservices.ToDtos(environment.ID),
                Name = environment.Name,
                Path = environment.Path,
                TenantId = environment.TenantID,
                TenantName = environment.Tenant.Name,
                SimulateTime = environment.SimulateTime
            };
        }
        #endregion
    }
}
