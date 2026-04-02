using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flowgrate;

public static class ManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static MigrationManifest Build(Migration migration, string migrationName)
    {
        Schema.Reset();
        migration.Up();
        var up = Schema.Operations.Select(ToOperation).ToList();

        Schema.Reset();
        migration.Down();
        var down = Schema.Operations.Select(ToOperation).ToList();

        Schema.Reset();

        return new MigrationManifest(migrationName, up, down);
    }

    public static string Serialize(MigrationManifest manifest) =>
        JsonSerializer.Serialize(manifest, JsonOptions);

    private static ManifestOperation ToOperation(SchemaOperation op) => op.Type switch
    {
        SchemaOperationType.Create => new ManifestOperation(
            Action: "create_table",
            Table: op.TableName,
            Columns: op.Blueprint!.Columns.Select(ToColumn).ToList(),
            Indexes: op.Blueprint.Indexes.Count > 0 ? op.Blueprint.Indexes.Select(ToIndex).ToList() : null,
            ForeignKeys: op.Blueprint.ForeignKeys.Count > 0 ? op.Blueprint.ForeignKeys.Select(ToForeignKey).ToList() : null
        ),
        SchemaOperationType.Alter => new ManifestOperation(
            Action: "alter_table",
            Table: op.TableName,
            Columns: op.Blueprint!.Columns.Select(ToColumn).ToList()
        ),
        SchemaOperationType.Drop => new ManifestOperation(
            Action: "drop_table",
            Table: op.TableName,
            IfExists: op.IfExists
        ),
        _ => throw new ArgumentOutOfRangeException(nameof(op.Type), op.Type, null)
    };

    private static ManifestColumn ToColumn(ColumnDefinition col) => new(
        Name: col.Name,
        Type: ColumnTypeToString(col.Type),
        ColumnAction: col.Action switch
        {
            ColumnAction.Add => "add",
            ColumnAction.Change => "change",
            ColumnAction.Drop => "drop",
            _ => "add"
        },
        Length: col.Length,
        Precision: col.Precision,
        Scale: col.Scale,
        Nullable: col.IsNullable,
        Primary: col.IsPrimary,
        AutoIncrement: col.IsAutoIncrement,
        Default: col.HasDefault && !col.IsDefaultExpression ? col.DefaultValue : null,
        DefaultExpression: col.HasDefault && col.IsDefaultExpression ? col.DefaultValue?.ToString() : null,
        Comment: col.Comment
    );

    private static ManifestIndex ToIndex(IndexDefinition idx) => new(
        Columns: idx.Columns,
        Unique: idx.IsUnique,
        Name: idx.Name
    );

    private static ManifestForeignKey ToForeignKey(ForeignKeyDefinition fk) => new(
        Column: fk.Column,
        ReferencesTable: fk.ReferencedTable,
        ReferencesColumn: fk.ReferencedColumn,
        OnUpdate: fk.OnUpdate,
        OnDelete: fk.OnDelete
    );

    private static string ColumnTypeToString(ColumnType type) => type switch
    {
        ColumnType.String      => "string",
        ColumnType.Text        => "text",
        ColumnType.SmallInteger => "small_integer",
        ColumnType.Integer     => "integer",
        ColumnType.BigInteger  => "big_integer",
        ColumnType.Decimal     => "decimal",
        ColumnType.Float       => "float",
        ColumnType.Double      => "double",
        ColumnType.Boolean     => "boolean",
        ColumnType.Date        => "date",
        ColumnType.Time        => "time",
        ColumnType.Timestamp   => "timestamp",
        ColumnType.Uuid        => "uuid",
        ColumnType.Json        => "json",
        ColumnType.Jsonb       => "jsonb",
        ColumnType.Binary      => "binary",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
