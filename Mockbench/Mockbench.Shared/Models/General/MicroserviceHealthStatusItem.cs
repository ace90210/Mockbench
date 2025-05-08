namespace Mockbench.Shared.Models.General;

public class MicroserviceHealthStatusItem
{
    public int Id { get; set; }

    public string Path { get; set; }
        
    public string Name { get; set; }

    public bool Enabled { get; set; } = true;
    
    public string TenantName { get; set; }

    public string EnvironmentName { get; set; }

    public string TargetUrl { get; set; }
}