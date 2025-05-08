using System;

namespace Mockbench.Shared.Models.Timetravel
{
    public class UpdateTimeTravelDto
    {
        public DateTime? Time { get; set; }

        public TimeTravelScope Scope { get; set; }
    }
}

namespace Mockbench.Shared
{
    public enum TimeTravelScope
    {
        Tenant,
        Environment,
        Microservice,
        Endpoint
    }
}