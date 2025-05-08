using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.General;
using Mockbench.Shared.Models.Microservice;

namespace Mockbench.Shared.Helper;

public static class HttpHelpers
{
    public static List<HeaderItem> GetResponseHeadersToAdd(MicroserviceResultDto microservice, IEnumerable<HeaderItem> responseHeaders)
    {
        var headersToAdd = new List<HeaderItem>();
        
        foreach (var header in responseHeaders)
        {
            if (header.Value != default(StringValues))
            {
                switch (microservice.HeadersMode)
                {
                    case HeadersMode.All:
                    {
                        headersToAdd.Add(new HeaderItem( header.Name, string.Join(",", header.Value)));
                    } break;
                    case HeadersMode.UserDefined:
                    {
                        var matchingHeader = microservice.Headers?.Any(h =>
                            h.Enabled && h.Incoming && h.Name.ToUpper().Equals(header.Name.ToUpper()));
                        if (matchingHeader ?? false)
                        {
                            headersToAdd.Add(new HeaderItem( header.Name, string.Join(",", header.Value)));
                        }
                    } break;
                }
            }
        }

        return headersToAdd;
    }
}