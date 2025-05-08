using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Mockbench.Shared.Helper;

namespace Mockbench.Shared.Models.Environment
{
    public class BaseEnvironmentDto : IValidatableObject
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "field exceeded max length {0}")]
        public string Name { get; set; }

        public bool Enabled { get; set; } = true;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Path is required")]
        [MaxLength(150, ErrorMessage = "field exceeded max length {0}")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Must have alpha numeric characters only")]
        public string Path { get; set; }

        public int TenantId { get; set; }
        
        public string TenantName { get; set; }

        public DateTime? SimulateTime { get; set; }

        [MaxLength(250, ErrorMessage = "field exceeded max length {0}")]
        public string DefaultHealthCheckUrl { get; set; }

        public bool IsValid(IDictionary<object, object> validationDictionary = null)
        {
            return GeneralHelper.IsValidFullObject(this, new ValidationContext(this, null, validationDictionary), null);
        }
        
        /// <summary>
        /// Validates tenant
        /// </summary>
        /// <param name="validationContext">a validation context that contains a dictionary of existing tenant paths</param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((validationContext.MemberName == null || validationContext.MemberName.Equals("Path")) && validationContext.Items.TryGetValue("Path", out object existingPathObjects))
            {
                var existingPaths = (IEnumerable<string>)existingPathObjects;
                if (existingPaths.Any(path => string.Equals(path, Path, StringComparison.CurrentCultureIgnoreCase)))
                {
                    yield return new ValidationResult($"Path \"{Path}\" already taken by existing environment in tenant, please try another", new[] { "Path" });
                }
            }
            if ((validationContext.MemberName == null || validationContext.MemberName.Equals("Name")) && validationContext.Items.TryGetValue("Name", out object existingNameObjects))
            {
                var existingNames = (IEnumerable<string>)existingNameObjects;

                if (existingNames.Any(name => string.Equals(name, Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    yield return new ValidationResult($"Name \"{Name}\" already taken by existing environment in tenant, please try another", new[] { "Name" });
                }
            }

            if (!string.IsNullOrEmpty(DefaultHealthCheckUrl) && (validationContext.MemberName == null || validationContext.MemberName.Equals("DefaultHealthCheckUrl")) )
            {
                if (!Uri.IsWellFormedUriString(DefaultHealthCheckUrl, UriKind.Absolute))
                {
                    yield return new ValidationResult("Default health check must be a valid url",
                        new[] { "DefaultHealthCheckUrl" });
                }
            }
        }
    }
}
