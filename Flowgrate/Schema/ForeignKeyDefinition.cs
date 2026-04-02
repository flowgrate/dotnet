namespace Flowgrate;

public class ForeignKeyDefinition
{
    public string Column { get; init; } = "";
    public string ReferencedTable { get; init; } = "";
    public string ReferencedColumn { get; init; } = "id";
    public string? OnUpdate { get; set; }
    public string? OnDelete { get; set; }
}
