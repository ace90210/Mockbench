using Mockbench.Shared.Models.Enum;

namespace Mockbench.Shared.Models.Utility
{
    public class PingTestResult
    {
        public TestUrlResult TestUrlResult { get; set; }

        public int? Latency { get; set; }

        public string Message { get; set; }
    }
}
