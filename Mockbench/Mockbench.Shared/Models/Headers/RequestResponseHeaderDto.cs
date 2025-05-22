// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Mockbench.Shared.Models.Headers;

public class MockResponseHeaderDto : IValidatableObject
{
    public int Id { get; set; }
    
    [MaxLength(150, ErrorMessage = "Header name exceeded max length {0}")]
    public string Name { get; set; }

    [MaxLength(5000, ErrorMessage = "Header value exceeded max length {0}")]
    public string Value { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Enumerable.Empty<ValidationResult>();
    }
}