using Mockbench.Shared.Models.Environment;

namespace Mockbench.Data.Mappers
{
    public static class EnvironmentMappers
    {
        #region Basic Environment Mappers
        public static EnvironmentDto? ToBaseEnvironmentDto(this Models.Environment environment)
        {
            return environment == null ? null : new EnvironmentDto()
            {
                Id = environment.ID,
                Enabled = environment.Enabled,
                DefaultHealthCheckUrl = environment.DefaultHealthCheckUrl,
                Name = environment.Name,
                Path = environment.Path,
                SimulateTime = environment.SimulateTime
            };
        }
        #endregion
    }
}
