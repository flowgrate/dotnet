using System.Text.Json.Serialization;

namespace Flowgrate;

public record MigrationManifest(
    string Migration,
    IReadOnlyList<ManifestOperation> Up,
    IReadOnlyList<ManifestOperation> Down
);

public record ManifestOperation(
    string Action,
    string Table,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool IfExists = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<ManifestColumn>? Columns = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<ManifestIndex>? Indexes = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<ManifestForeignKey>? ForeignKeys = null
);

public record ManifestColumn(
    string Name,
    string Type,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    string ColumnAction = "add",
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? Length = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? Precision = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? Scale = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool Nullable = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool Primary = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool AutoIncrement = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    object? Default = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? DefaultExpression = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Comment = null
);

public record ManifestIndex(
    string[] Columns,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    bool Unique = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Name = null
);

public record ManifestForeignKey(
    string Column,
    string ReferencesTable,
    string ReferencesColumn = "id",
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? OnUpdate = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? OnDelete = null
);
