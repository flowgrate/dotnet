namespace Flowgrate;

public enum ColumnAction { Add, Change, Drop }

public enum ColumnType
{
    String,
    Text,
    Integer,
    BigInteger,
    Float,
    Boolean,
    Date,
    Timestamp,
}

public class ColumnDefinition
{
    public string Name { get; init; } = "";
    public ColumnType Type { get; set; }
    public int? Length { get; set; }
    public bool IsNullable { get; set; }
    public bool HasDefault { get; set; }
    public object? DefaultValue { get; set; }
    public string? Comment { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsAutoIncrement { get; set; }
    public ColumnAction Action { get; init; } = ColumnAction.Add;
}
