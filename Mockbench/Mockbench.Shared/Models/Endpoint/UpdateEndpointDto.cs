using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Shared.Models.Endpoint
{
    public class UpdateEndpointDto : IValidatableObject
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(500, ErrorMessage = "Endpoint too long. Maximum length is 500")]
        public string FromUrl { get; set; } = string.Empty;

        public bool ExactUrlMatch { get; set; }

        public bool ExpectAuthHeader { get; set; }

        public MockBehaviour MockBehaviour { get; set; }

        public bool Enabled { get; set; } = true;

        public RestType RestType { get; set; }

        public DateTime? SimulateTime { get; set; }
        
        [MaxLength(10_000_000, ErrorMessage = "Body exceeded max length {0}")]
        public string FromBody { get; set; }

        public DateTime? CreatedUtc { get; set; }

        public List<MockResponseDto> Responses { get; set; }

        public List<QueryParameterDto> QueryParameters { get; set; }
        
        public List<EndpointHeaderDto> EndpointHeaders { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}
