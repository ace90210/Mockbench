using Mockbench.Shared.Models.Enum;
using System.Net;

namespace Mockbench.Shared.Models.Response;

public interface IMockResponse
{
    string? Body { get; set; }
    HttpStatusCode Code { get; set; }
    string? ContentType { get; set; }
    DateTime? CreatedUtc { get; set; }
    SupportedEncodingType Encoding { get; set; }
}