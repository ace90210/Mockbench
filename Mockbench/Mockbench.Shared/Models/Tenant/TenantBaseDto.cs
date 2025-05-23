using System.ComponentModel.DataAnnotations;
using Mockbench.Shared.Models.Endpoint;

namespace Mockbench.Shared.Models.Tenant;

public class TenantBaseDto : IValidatableObject
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "field exceeded max length {0}")]
    public string Name { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Path is required")]
    [MaxLength(150, ErrorMessage = "field exceeded max length {0}")]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Must have alpha numeric characters only")]
    public string Path { get; set; }

    public DateTime? SimulateTime { get; set; }

    public List<EndpointDto>? Endpoints { get; set; }

    public List<TenantVariableDto>? Variables { get; set; }

    /// <summary>
    /// Validates tenant
    /// </summary>
    /// <param name="validationContext">a validation context that contains a dictionary of existing tenant paths</param>
    /// <returns></returns>
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if ((validationContext.MemberName == null || validationContext.MemberName.Equals("Path")) && validationContext.Items.TryGetValue("Path", out object existingPathObjects))
        {
            var existingPaths = (IEnumerable<string>)existingPathObjects;
            if (existingPaths.Any(path => string.Equals(path, Path, StringComparison.CurrentCultureIgnoreCase)))
            {
                yield return new ValidationResult($"Path \"{Path}\" already taken by existing tenant, please try another", new[] { "Path" });
            }
        }
        if ((validationContext.MemberName == null || validationContext.MemberName.Equals("Name")) && validationContext.Items.TryGetValue("Name", out object existingNameObjects))
        {
            var existingNames = (IEnumerable<string>)existingNameObjects;

            if (existingNames.Any(name => string.Equals(name, Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                yield return new ValidationResult($"Name \"{Name}\" already taken by existing tenant, please try another", new[] { "Name" });
            }
        }
    }
}
