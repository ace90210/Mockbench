namespace Mockbench.Shared.Models.General;

public class HeaderItem
{
    public string Name { get; set; }
    
    public string Value { get; set; }

    public HeaderItem()
    {
        
    }

    public HeaderItem(string name, string value)
    {
        Name = name;
        Value = value;
    }
}