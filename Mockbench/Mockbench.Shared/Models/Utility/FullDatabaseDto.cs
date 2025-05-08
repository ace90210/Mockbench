using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Tenant;

namespace Mockbench.Shared.Models.Utility;

public class FullDatabaseDto : IValidatableObject
{
    public IEnumerable<TenantBase> Tenants { get; set; }

    public IEnumerable<string> AppliedMigrations { get; set; }
    
    public string DatabaseType { get; set; }

    public string CodeVersion { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var environmentResult in ValidateTenants(Tenants?.ToList(), validationContext)) yield return environmentResult;
    }
    

    private IEnumerable<ValidationResult> ValidateTenants(List<TenantBase> tenants, ValidationContext validationContext)
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
    
}