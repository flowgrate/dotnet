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
        _column.DefaultValue = value;
        return this;
    }

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

    // Методы установки типа — для использования после AddColumn / ChangeColumn
    public ColumnBuilder String(int length = 255)
    {
        _column.Type = ColumnType.String;
        _column.Length = length;
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

    public ColumnBuilder Float()
    {
        _column.Type = ColumnType.Float;
        return this;
    }

    public ColumnBuilder Text()
    {
        _column.Type = ColumnType.Text;
        return this;
    }

    public ColumnBuilder Boolean()
    {
        _column.Type = ColumnType.Boolean;
        return this;
    }
}
