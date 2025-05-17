namespace Mockbench.Data.Mappers
{
    public static class Mapper
    {
        public static readonly TenantMapper Tenant = new();
        public static readonly TenantClonerMapper TenantCloner = new();

        public static readonly EnvironmentMapper Environment = new();
        public static readonly EnvironmentClonerMapper EnvironmentCloner = new();

        public static readonly MicroserviceMapper Microservice = new();
        public static readonly MicroserviceClonerMapper MicroserviceCloner = new();
    }
}
