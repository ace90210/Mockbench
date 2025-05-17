using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Headers;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Mockbench.Shared.Models.Response
{
    public class UpdateMockResponseDto: IMockResponse
    {
        [MaxLength(250, ErrorMessage = "Description too long. Maximum length is 250")]
        public string? Description { get; set; }

        public HttpStatusCode Code { get; set; }

        public SupportedEncodingType Encoding { get; set; } = SupportedEncodingType.UTF8;

        [MaxLength(50, ErrorMessage = "Content type too long. Maximum length is 50")]
        public string? ContentType { get; set; } = "text/plain";

        public string? Body { get; set; }

        public int Priority { get; set; } = 100;

        public int FakeDelay { get; set; }

        public bool Enabled { get; set; } = true;

        [RegularExpression(@"^(?:(?:([01]?\d|2[0-3]):)?([0-5]?\d):)?([0-5]?\d)(?:\.(\d+))?$")]
        public TimeSpan Latency { get; set; }

        public DateTime? CreatedUtc { get; set; }

        public List<MockResponseHeaderDto> Headers { get; set; } = new ();
    }
}
