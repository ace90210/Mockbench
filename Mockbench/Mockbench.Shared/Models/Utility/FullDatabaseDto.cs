using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.Tenant;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Shared.Models.Utility;

public class FullDatabaseDto : IValidatableObject
{
    public IEnumerable<TenantBase> Tenants { get; set; }

    public IEnumerable<EnvironmentDto> Environments { get; set; }


    public IEnumerable<FullMicroserviceDto> Microservices { get; set; }

    public IEnumerable<string> AppliedMigrations { get; set; }
    
    public string DatabaseType { get; set; }

    public string CodeVersion { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var validationResult in ValidateTenants(Tenants?.ToList(), validationContext)) yield return validationResult;

        foreach (var validationResult in ValidateEnvironments(Environments?.ToList(), validationContext)) yield return validationResult;

        foreach (var validationResult in ValidateMicroservices(Microservices?.ToList(), validationContext)) yield return validationResult;
    }

    private IEnumerable<ValidationResult> ValidateTenants(List<TenantBase> envinoments, ValidationContext validationContext)
    {
        if (envinoments?.Count > 0)
        {
            foreach (var tenant in envinoments)
            {
                var validationResults = GeneralHelper.ValidateFullObject(tenant, new ValidationContext(tenant, null, validationContext.Items));

                foreach (var validationResult in validationResults)
                {
                    yield return validationResult;
                }
            }
        }
    }

    private IEnumerable<ValidationResult> ValidateEnvironments(List<EnvironmentDto> tenants, ValidationContext validationContext)
    {
        if (tenants?.Count > 0)
        {
            foreach (var tenant in tenants)
            {
                var validationResults = GeneralHelper.ValidateFullObject(tenant, new ValidationContext(tenant, null, validationContext.Items));

                foreach (var validationResult in validationResults)
                {
                    yield return validationResult;
                }
            }
        }
    }

    private IEnumerable<ValidationResult> ValidateMicroservices(List<FullMicroserviceDto> microservices, ValidationContext validationContext)
    {
        if (microservices?.Count > 0)
        {
            foreach (var tenant in microservices)
            {
                var validationResults = GeneralHelper.ValidateFullObject(tenant, new ValidationContext(tenant, null, validationContext.Items));

                foreach (var validationResult in validationResults)
                {
                    yield return validationResult;
                }
            }
        }
    }

}