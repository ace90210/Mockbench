namespace Mockbench.Shared.Models.Microservice;

public class MicroserviceSearchResultDto : MicroserviceResultDto
{
    public int TenantId { get; set; }
    
    public string TenantName { get; set; }

    public int EnvironmentId { get; set; }
    
    public string EnvironmentName { get; set; }

    public int TotalEndpoints { get; set; }
}