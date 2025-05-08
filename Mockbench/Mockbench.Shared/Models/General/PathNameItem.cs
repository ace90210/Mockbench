namespace Mockbench.Shared.Models.General;

public class PathNameItem
{
    public string Name { get; set; }
    
    public string Path { get; set; }

    public PathNameItem(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public PathNameItem()
    {
        
    }
}