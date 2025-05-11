using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Models.Enum;

namespace Mockbench.Data.Models
{
    public class Endpoint
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = true)]
        [MaxLength(500)]
        public string FromUrl { get; set; } = string.Empty;

        public bool ExactUrlMatch { get; set; }

        public bool ExpectAuthHeader { get; set; }

        public MockBehaviour MockBehaviour { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        public RestType RestType { get; set; }

        public DateTime? SimulateTime { get; set; }

        public string? FromBody { get; set; }

        [Obsolete("Property 'Duration' should be used instead.")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long? TTLTicks { get; set; }

        // EF doesnt handle timespans greater than 24 hours correctly
        // Workaround to store ticks and convert
        [NotMapped]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public TimeSpan? TTL
        {
#pragma warning disable 618
            get
            {
                if (TTLTicks == null)
                    return null;
                return new TimeSpan((long)TTLTicks);
            }
            set { TTLTicks = value?.Ticks; }
#pragma warning restore 618
        }

        private DateTime _createdUtc = DateTime.Now;
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedUtc {
            get {
                return _createdUtc;
            }
            set
            {
                _createdUtc = value;
            }
        }
        
        public List<QueryParameter> QueryParameters { get; set; }

        public List<MockResponse> MockResponses { get; set; }

        public List<EndpointHeader> EndpointHeaders { get; set; }

        public int? TenantId { get; set; }

        public Tenant? Tenant { get; set; }

        public int? EnvironmentId { get; set; }

        public Environment? Environment{ get; set; }

        public int? MicroserviceId { get; set; }

        public Microservice? Microservice { get; set; }
    }
}