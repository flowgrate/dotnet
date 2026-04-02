namespace Flowgrate;

public enum SchemaOperationType { Create, Alter, Drop }

public class SchemaOperation
{
    public SchemaOperationType Type { get; init; }
    public string TableName { get; init; } = "";
    public Blueprint? Blueprint { get; init; }
    public bool IfExists { get; init; }
}

public static class Schema
{
    private static readonly List<SchemaOperation> _operations = new();

    public static IReadOnlyList<SchemaOperation> Operations => _operations.AsReadOnly();

    public static void Create(string table, Action<Blueprint> callback)
    {
        var blueprint = new Blueprint(table);
        callback(blueprint);
        _operations.Add(new SchemaOperation { Type = SchemaOperationType.Create, TableName = table, Blueprint = blueprint });
    }

    public static void Table(string table, Action<Blueprint> callback)
    {
        var blueprint = new Blueprint(table);
        callback(blueprint);
        _operations.Add(new SchemaOperation { Type = SchemaOperationType.Alter, TableName = table, Blueprint = blueprint });
    }

    public static void Drop(string table)
    {
        _operations.Add(new SchemaOperation { Type = SchemaOperationType.Drop, TableName = table });
    }

    public static void DropIfExists(string table)
    {
        _operations.Add(new SchemaOperation { Type = SchemaOperationType.Drop, TableName = table, IfExists = true });
    }

    internal static void Reset() => _operations.Clear();
}
