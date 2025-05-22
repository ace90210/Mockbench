using System.ComponentModel.DataAnnotations;

namespace Mockbench.Shared.Models.QueryParameters;

public class QueryParameterDto : IValidatableObject
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int Id { get; set; }

    [MaxLength(150, ErrorMessage = "Parameter name exceeded max length {0}")]
    public string Name { get; set; }

    [MaxLength(5000, ErrorMessage = "parameter value exceeded max length {0}")]
    public string Value { get; set; }

    public int OrderIndex { get; set; }

    public bool Ignore { get; set; }

    public int EndpointId { get; set; }

    public QueryParameterDto() {    }

    public QueryParameterDto(string queryStringItem)
    {
        var segments = queryStringItem?.Split("=");

        if (segments?.Length > 0)
        {
            Name = segments[0];

            if (segments.Length == 1)
            {
                Value = segments[1];
            }
            else
            {
                Value = string.Join(string.Empty, segments[1..Index.End]);
            }
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Enumerable.Empty<ValidationResult>();
    }
}