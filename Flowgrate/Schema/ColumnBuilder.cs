namespace Flowgrate;

public class ColumnBuilder
{
    private readonly Blueprint _blueprint;
    private readonly ColumnDefinition _column;
    private ForeignKeyDefinition? _foreignKey;

    internal ColumnBuilder(Blueprint blueprint, ColumnDefinition column)
    {
        _blueprint = blueprint;
        _column = column;
    }

    public ColumnBuilder Nullable()
    {
        _column.IsNullable = true;
        return this;
    }

    public ColumnBuilder Default(object value)
    {
        _column.HasDefault = true;
        _column.IsDefaultExpression = false;
        _column.DefaultValue = value;
        return this;
    }

    /// <summary>Sets a raw SQL expression as the column default (e.g. gen_random_uuid(), NOW()).</summary>
    public ColumnBuilder DefaultExpression(string expression)
    {
        _column.HasDefault = true;
        _column.IsDefaultExpression = true;
        _column.DefaultValue = expression;
        return this;
    }

    /// <summary>Sets DEFAULT gen_random_uuid() on a UUID column.</summary>
    public ColumnBuilder GeneratedUuid() => DefaultExpression("gen_random_uuid()");

    public ColumnBuilder Comment(string comment)
    {
        _column.Comment = comment;
        return this;
    }

    public ColumnBuilder Constrained(string table, string column = "id")
    {
        _foreignKey = new ForeignKeyDefinition
        {
            Column = _column.Name,
            ReferencedTable = table,
            ReferencedColumn = column
        };
        _blueprint.ForeignKeys.Add(_foreignKey);
        return this;
    }

    public ColumnBuilder OnUpdate(string action)
    {
        if (_foreignKey != null)
            _foreignKey.OnUpdate = action;
        return this;
    }

    public ColumnBuilder OnDelete(string action)
    {
        if (_foreignKey != null)
            _foreignKey.OnDelete = action;
        return this;
    }

    // Convenience: add a single-column unique index
    public ColumnBuilder Unique()
    {
        _blueprint.Indexes.Add(new IndexDefinition { Columns = [_column.Name], IsUnique = true });
        return this;
    }

    // --- Type setters (for use after AddColumn / ChangeColumn) ---

    public ColumnBuilder String(int length = 255)
    {
        _column.Type = ColumnType.String;
        _column.Length = length;
        return this;
    }

    public ColumnBuilder Text()
    {
        _column.Type = ColumnType.Text;
        return this;
    }

    public ColumnBuilder SmallInteger()
    {
        _column.Type = ColumnType.SmallInteger;
        return this;
    }

    public ColumnBuilder Integer()
    {
        _column.Type = ColumnType.Integer;
        return this;
    }

    public ColumnBuilder BigInteger()
    {
        _column.Type = ColumnType.BigInteger;
        return this;
    }

    public ColumnBuilder Decimal(int precision = 8, int scale = 2)
    {
        _column.Type = ColumnType.Decimal;
        _column.Precision = precision;
        _column.Scale = scale;
        return this;
    }

    public ColumnBuilder Float()
    {
        _column.Type = ColumnType.Float;
        return this;
    }

    public ColumnBuilder Double()
    {
        _column.Type = ColumnType.Double;
        return this;
    }

    public ColumnBuilder Boolean()
    {
        _column.Type = ColumnType.Boolean;
        return this;
    }

    public ColumnBuilder Date()
    {
        _column.Type = ColumnType.Date;
        return this;
    }

    public ColumnBuilder Time()
    {
        _column.Type = ColumnType.Time;
        return this;
    }

    public ColumnBuilder Timestamp()
    {
        _column.Type = ColumnType.Timestamp;
        return this;
    }

    public ColumnBuilder Uuid()
    {
        _column.Type = ColumnType.Uuid;
        return this;
    }

    public ColumnBuilder Json()
    {
        _column.Type = ColumnType.Json;
        return this;
    }

    /// <summary>Binary JSON with indexing. PostgreSQL only.</summary>
    public ColumnBuilder Jsonb()
    {
        _column.Type = ColumnType.Jsonb;
        return this;
    }

    public ColumnBuilder Binary()
    {
        _column.Type = ColumnType.Binary;
        return this;
    }
}
