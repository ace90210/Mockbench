using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Mockbench.Data.Models.Headers;
using Mockbench.Shared.Helper;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Response;

namespace Mockbench.Data.Models
{
    public class MockResponse : IMockResponse
    {
        [Key]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public HttpStatusCode Code { get; set; }

        public SupportedEncodingType Encoding { get; set; } = SupportedEncodingType.UTF8;

        [MaxLength(50)]
        public string? ContentType { get; set; } = "text/plain";

        public string? Body { get; set; }

        public int EndpointId { get; set; }

        public string? Checksum => ChecksumHelpers.CreateDefaultChecksum(this);

        public int Priority { get; set; } = 100;

        public int FakeDelay { get; set; }

        public bool Enabled { get; set; } = true;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<ResponseHeader> Headers { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Endpoint? Endpoint { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public TimeSpan Latency { get; set; }

        private DateTime? _createdUtc = DateTime.Now;
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedUtc
        {
            get
            {
                return _createdUtc;
            }
            set
            {
                _createdUtc = value;
            }
        }
    }
}