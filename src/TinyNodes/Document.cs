namespace TinyNodes;

public class Document
{
    public string Name { get; set; }
    public List<NameSpace> NameSpaces { get; } = new();
}


public class NameSpace
{
    public string Name { get; set; }
    public List<NameSpace> NameSpaces { get; } = new();
    public List<Function> Functions { get; } = new();
}

public class Function
{
    public string Name { get; set; }
}
