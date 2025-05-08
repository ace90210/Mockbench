using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Mockbench.Shared.Models.General
{
    public class HttpRequestDto
    {
        public DateTime Timestamp { get; set; }
        
        public string Endpoint { get; set; }

        public string Body { get; set; }

        public string QueryString { get; set; }

        public string HttpMethod { get; set; }

        public Dictionary<string, string> Headers { get; set; }
    }
}
