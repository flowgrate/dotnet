    namespace Flowgrate;

public class Blueprint
{
    public string TableName { get; }
    internal List<ColumnDefinition> Columns { get; } = new();
    internal List<IndexDefinition> Indexes { get; } = new();
    internal List<ForeignKeyDefinition> ForeignKeys { get; } = new();

    internal Blueprint(string tableName)
    {
        TableName = tableName;
    }

    public ColumnBuilder Id()
    {
        var col = new ColumnDefinition { Name = "id", Type = ColumnType.BigInteger, IsPrimary = true, IsAutoIncrement = true };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Integer(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Integer };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder BigInteger(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.BigInteger };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Float(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Float };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Boolean(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Boolean };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Date(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Date };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Timestamp(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Timestamp };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder String(string name, int length = 255)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.String, Length = length };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Text(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Text };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder ForeignId(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.BigInteger };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public void Timestamps()
    {
        Columns.Add(new ColumnDefinition { Name = "created_at", Type = ColumnType.Timestamp, HasDefault = true, DefaultValue = "NOW()" });
        Columns.Add(new ColumnDefinition { Name = "updated_at", Type = ColumnType.Timestamp, HasDefault = true, DefaultValue = "NOW()" });
    }

    public IndexBuilder Unique(params string[] columns)
    {
        var index = new IndexDefinition { Columns = columns, IsUnique = true };
        Indexes.Add(index);
        return new IndexBuilder(index);
    }

    public IndexBuilder Index(params string[] columns)
    {
        var index = new IndexDefinition { Columns = columns, IsUnique = false };
        Indexes.Add(index);
        return new IndexBuilder(index);
    }

    // ALTER TABLE
    public ColumnBuilder AddColumn(string name)
    {
        var col = new ColumnDefinition { Name = name, Action = ColumnAction.Add };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder ChangeColumn(string name)
    {
        var col = new ColumnDefinition { Name = name, Action = ColumnAction.Change };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public void DropColumn(string name)
    {
        Columns.Add(new ColumnDefinition { Name = name, Action = ColumnAction.Drop });
    }
}
