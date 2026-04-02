namespace Flowgrate;

public enum ColumnAction { Add, Change, Drop }

public enum ColumnType
{
    String,
    Text,
    Integer,
    SmallInteger,
    BigInteger,
    Decimal,
    Float,
    Double,
    Boolean,
    Date,
    Time,
    Timestamp,
    Uuid,
    Json,
    /// <summary>PostgreSQL only. Binary JSON with indexing support.</summary>
    Jsonb,
    Binary,
}

public class ColumnDefinition
{
    public string Name { get; init; } = "";
    public ColumnType Type { get; set; }
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool HasDefault { get; set; }
    public bool IsDefaultExpression { get; set; }
    public object? DefaultValue { get; set; }
    public string? Comment { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsAutoIncrement { get; set; }
    public ColumnAction Action { get; init; } = ColumnAction.Add;
}
