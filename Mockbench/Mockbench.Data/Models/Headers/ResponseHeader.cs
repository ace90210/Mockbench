namespace Mockbench.Data.Models.Headers
{
    public class ResponseHeader : BaseHeader
    {
        public string Value { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int MockResponseID { get; set; }
    }
}
