namespace Mockbench.Data.Models.Headers
{
    public class EndpointHeader : BaseHeader
    {
        public string Value { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int EndpointId { get; set; }
    }
}
