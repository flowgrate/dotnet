namespace Flowgrate;

public class IndexDefinition
{
    public string[] Columns { get; init; } = [];
    public bool IsUnique { get; init; }
    public string? Name { get; set; }
}
