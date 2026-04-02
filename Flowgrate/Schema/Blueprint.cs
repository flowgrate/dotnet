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

    // --- Integer types ---

    public ColumnBuilder Id()
    {
        var col = new ColumnDefinition { Name = "id", Type = ColumnType.BigInteger, IsPrimary = true, IsAutoIncrement = true };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder SmallInteger(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.SmallInteger };
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

    // --- Numeric types ---

    public ColumnBuilder Decimal(string name, int precision = 8, int scale = 2)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Decimal, Precision = precision, Scale = scale };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Float(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Float };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Double(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Double };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    // --- String types ---

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

    // --- UUID ---

    public ColumnBuilder Uuid(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Uuid };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    // --- JSON ---

    public ColumnBuilder Json(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Json };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    /// <summary>Binary JSON with indexing. PostgreSQL only.</summary>
    public ColumnBuilder Jsonb(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Jsonb };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    // --- Other types ---

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

    public ColumnBuilder Time(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Time };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Timestamp(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Timestamp };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    public ColumnBuilder Binary(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.Binary };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    // --- Foreign keys ---

    public ColumnBuilder ForeignId(string name)
    {
        var col = new ColumnDefinition { Name = name, Type = ColumnType.BigInteger };
        Columns.Add(col);
        return new ColumnBuilder(this, col);
    }

    // --- Convenience helpers ---

    /// <summary>Adds created_at and updated_at timestamp columns.</summary>
    public void Timestamps()
    {
        Columns.Add(new ColumnDefinition { Name = "created_at", Type = ColumnType.Timestamp, HasDefault = true, IsDefaultExpression = true, DefaultValue = "NOW()" });
        Columns.Add(new ColumnDefinition { Name = "updated_at", Type = ColumnType.Timestamp, HasDefault = true, IsDefaultExpression = true, DefaultValue = "NOW()" });
    }

    /// <summary>Adds a nullable deleted_at timestamp for soft deletes.</summary>
    public void SoftDeletes()
    {
        Columns.Add(new ColumnDefinition { Name = "deleted_at", Type = ColumnType.Timestamp, IsNullable = true });
    }

    /// <summary>Adds a nullable remember_token VARCHAR(100) column.</summary>
    public void RememberToken()
    {
        Columns.Add(new ColumnDefinition { Name = "remember_token", Type = ColumnType.String, Length = 100, IsNullable = true });
    }

    /// <summary>
    /// Adds {name}_id (BIGINT NOT NULL) and {name}_type (VARCHAR(255) NOT NULL)
    /// for polymorphic relationships, plus a composite index on both columns.
    /// </summary>
    public void Polymorphic(string name)
    {
        Columns.Add(new ColumnDefinition { Name = $"{name}_id", Type = ColumnType.BigInteger });
        Columns.Add(new ColumnDefinition { Name = $"{name}_type", Type = ColumnType.String, Length = 255 });
        Indexes.Add(new IndexDefinition { Columns = [$"{name}_id", $"{name}_type"], IsUnique = false });
    }

    /// <summary>
    /// Same as Polymorphic but both columns are nullable (for optional relationships).
    /// </summary>
    public void NullablePolymorphic(string name)
    {
        Columns.Add(new ColumnDefinition { Name = $"{name}_id", Type = ColumnType.BigInteger, IsNullable = true });
        Columns.Add(new ColumnDefinition { Name = $"{name}_type", Type = ColumnType.String, Length = 255, IsNullable = true });
        Indexes.Add(new IndexDefinition { Columns = [$"{name}_id", $"{name}_type"], IsUnique = false });
    }

    // --- Indexes ---

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

    // --- ALTER TABLE ---

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
