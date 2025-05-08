using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mockbench.Shared.Helper;

public static class GeneralHelper
{
    public static List<ValidationResult> ValidateFullObject<T>(T obj, ValidationContext validationContext, bool allProperties = true) where T : IValidatableObject
    {
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContext.Items),
            validationResults, allProperties);
        
        // validator will skip validate method when no errors on attributes to run it explicitly
        if (validationResults.Count == 0)
        {
            var additionalValidationResults = obj.Validate(validationContext).ToList();

            if (additionalValidationResults.Count > 0)
            {
                validationResults.AddRange(additionalValidationResults);
            }
        }

        return validationResults;
    }
    
    /// <summary>
    /// tries to validate all properties without exception (does not bail out early if possible), returns true if object is valid
    /// </summary>
    /// <param name="obj">The object to validate</param>
    /// <param name="validationContext">a validation context to provide to the object</param>
    /// <param name="validationResults">the validation results to be returned</param>
    /// <param name="allProperties">(optional) sets the all properties on <see cref="Validator" /> to specified value, defaults to true</param>
    /// <typeparam name="T">The Type, must inherit from <see cref="IValidatableObject"/></typeparam>
    /// <returns>true is validation passes</returns>
    public static bool TryValidateFullObject<T>(T obj, ValidationContext validationContext, 
        List<ValidationResult> validationResults, bool allProperties = true) where T : IValidatableObject
    {
        if (validationResults == null)
        {
            validationResults = new List<ValidationResult>();
        }

        Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContext.Items),
            validationResults, allProperties);

        // validator will skip validate method when no errors on attributes to run it explicitly
        if (validationResults.Count == 0)
        {
            var additionalValidationResults = obj.Validate(validationContext).ToList();
            if (additionalValidationResults.Count > 0)
            {
                validationResults.AddRange(additionalValidationResults);
            }
        }

        return !validationResults.Any();
    }
    /// <summary>
    /// tries to validate all properties without exception (does not bail out early if possible) but as soon as possible returns false when a error is found, returns true if object is valid.
    /// </summary>
    /// <param name="obj">The object to validate</param>
    /// <param name="validationContext">a validation context to provide to the object</param>
    /// <param name="validationResults">the validation results to be returned</param>
    /// <param name="allProperties">(optional) sets the all properties on <see cref="Validator" /> to specified value, defaults to true</param>
    /// <typeparam name="T">The Type, must inherit from <see cref="IValidatableObject"/></typeparam>
    /// <returns>true is validation passes</returns>
    public static bool IsValidFullObject<T>(T obj, ValidationContext validationContext, 
        List<ValidationResult> validationResults, bool allProperties = true) where T : IValidatableObject
    {
        if (validationResults == null)
        {
            validationResults = new List<ValidationResult>();
        }

        Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContext.Items),
            validationResults, allProperties);

        if (validationResults.Any())
        {
            return false;
        }

        var additionalValidationResults = obj.Validate(validationContext).ToList();

        if (additionalValidationResults.Count > 0)
        {
            return false;
        }

        return true;
    }
}