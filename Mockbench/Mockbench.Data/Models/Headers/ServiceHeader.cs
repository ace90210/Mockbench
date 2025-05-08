namespace Mockbench.Data.Models.Headers
{
    public class ServiceHeader : BaseHeader
    {
        public bool Enabled { get; set; }

        public bool Incoming { get; set; }

        public bool Outgoing { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int MicroserviceID { get; set; }
    }
}
