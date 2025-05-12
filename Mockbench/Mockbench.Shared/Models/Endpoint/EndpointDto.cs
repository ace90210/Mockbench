using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;
using System.ComponentModel.DataAnnotations;

namespace Mockbench.Shared.Models.Endpoint
{
    public class EndpointDto : ICopyTo<EndpointDto>, IValidatableObject
    {
        public int Id { get; set; }
      
        [Required(AllowEmptyStrings = true)]
        [MaxLength(500, ErrorMessage = "Endpoint too long. Maximum length is 500")]
        public string FromUrl { get; set; } = string.Empty;

        public bool ExactUrlMatch { get; set; }

        public bool ExpectAuthHeader { get; set; }

        public MockBehaviour MockBehaviour { get; set; } = MockBehaviour.AutoMockWithProxy;

        public bool Enabled { get; set; } = true;

        public RestType? RestType { get; set; } = Mockbench.Shared.Models.Enum.RestType.GET;

        public TimeSpan? Ttl { get; set; }
        
        [MaxLength(10_000_000, ErrorMessage = "Body exceeded max length {0}")]
        public string? FromBody { get; set; }

        public DateTime? SimulateTime { get; set; }

        public List<MockResponseDto>? MockResponses { get; set; }

        public DateTime CreatedUtc { get; set; }

        public int? TenantId { get; set; }

        public int? EnvironmentId { get; set; }

        public int? MicroserviceId { get; set; }

        public List<EndpointHeaderDto> EndpointHeaders { get; set; }

        public List<QueryParameterDto> QueryParameters { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public int GetId()
        {
            return Id;
        }

        public EndpointDto CopyTo(EndpointDto target)
        {
            target.Id = Id;
            target.FromUrl = FromUrl;
            target.ExactUrlMatch = ExactUrlMatch;
            target.ExpectAuthHeader = ExpectAuthHeader;
            target.MockBehaviour = MockBehaviour;
            target.Enabled = Enabled;
            target.RestType = RestType;
            target.Ttl = Ttl;
            target.FromBody = FromBody;
            target.SimulateTime = SimulateTime;
            target.CreatedUtc = CreatedUtc;
            target.MicroserviceId = MicroserviceId;
            
            target.MockResponses = new List<MockResponseDto>();
            foreach(var response in MockResponses)
            {
                target.MockResponses.Add(response.CopyTo(new MockResponseDto()));
            }
            
            if(target.EndpointHeaders == null && EndpointHeaders is not null && EndpointHeaders.Count > 0)
                throw new ArgumentNullException($"{nameof(EndpointDto)} Headers: Cannot copy to a null target");

            if (EndpointHeaders is not null && EndpointHeaders.Count > 0)
            {
                if (target.EndpointHeaders == null)
                {
                    target.EndpointHeaders = EndpointHeaders;
                }
                else
                {
                    target.EndpointHeaders.Clear();
                    target.EndpointHeaders.AddRange(EndpointHeaders);
                }
            }
            
            if(target.QueryParameters == null && QueryParameters is not null && QueryParameters.Count > 0)
                throw new ArgumentNullException($"{nameof(EndpointDto)} Query Parameters: Cannot copy to a null target");

            if (QueryParameters is not null && QueryParameters.Count > 0)
            {
                if (target.QueryParameters == null)
                {
                    target.QueryParameters = QueryParameters;
                }
                else
                {
                    target.QueryParameters.Clear();
                    target.QueryParameters.AddRange(QueryParameters);
                }
            }

            return target;
        }
    }
}
