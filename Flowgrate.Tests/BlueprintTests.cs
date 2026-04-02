using Flowgrate;

namespace Flowgrate.Tests;

public class BlueprintTests
{
    public BlueprintTests()
    {
        Schema.Reset();
    }

    [Fact]
    public void Create_builds_columns_indexes_and_fk()
    {
        Schema.Create("nm_statistics", table =>
        {
            table.Id();
            table.Integer("views").Nullable().Comment("Количество просмотров");
            table.Integer("clicks").Nullable().Comment("Количество кликов");
            table.Float("ctr").Nullable();
            table.Boolean("need_update").Default(false);
            table.Date("date");
            table.ForeignId("campaign_id").Constrained("campaigns").OnUpdate("cascade").OnDelete("cascade");
            table.Timestamps();

            table.Unique("campaign_id", "date");
            table.Index("campaign_id", "date");
        });

        var op = Schema.Operations.Single();
        var bp = op.Blueprint!;

        Assert.Equal(SchemaOperationType.Create, op.Type);
        Assert.Equal("nm_statistics", op.TableName);
        Assert.Equal(9, bp.Columns.Count); // id, views, clicks, ctr, need_update, date, campaign_id, created_at, updated_at

        var views = bp.Columns.First(c => c.Name == "views");
        Assert.True(views.IsNullable);
        Assert.Equal("Количество просмотров", views.Comment);

        var needUpdate = bp.Columns.First(c => c.Name == "need_update");
        Assert.True(needUpdate.HasDefault);
        Assert.Equal(false, needUpdate.DefaultValue);

        var fk = bp.ForeignKeys.Single();
        Assert.Equal("campaign_id", fk.Column);
        Assert.Equal("campaigns", fk.ReferencedTable);
        Assert.Equal("cascade", fk.OnUpdate);
        Assert.Equal("cascade", fk.OnDelete);

        Assert.Equal(2, bp.Indexes.Count);
        Assert.True(bp.Indexes[0].IsUnique);
        Assert.False(bp.Indexes[1].IsUnique);
    }

    [Fact]
    public void DropIfExists_sets_IfExists_flag()
    {
        Schema.DropIfExists("nm_statistics");

        var op = Schema.Operations.Single();
        Assert.Equal(SchemaOperationType.Drop, op.Type);
        Assert.True(op.IfExists);
    }

    [Fact]
    public void Alter_table_change_column_type()
    {
        Schema.Table("popular_keywords", table =>
        {
            table.ChangeColumn("keyword").String(500).Nullable().Comment("Ключевая фраза");
        });

        var op = Schema.Operations.Single();
        var col = op.Blueprint!.Columns.Single();

        Assert.Equal(SchemaOperationType.Alter, op.Type);
        Assert.Equal(ColumnAction.Change, col.Action);
        Assert.Equal(ColumnType.String, col.Type);
        Assert.Equal(500, col.Length);
        Assert.True(col.IsNullable);
    }

    [Fact]
    public void New_numeric_types_are_set_correctly()
    {
        Schema.Create("products", table =>
        {
            table.SmallInteger("level");
            table.Decimal("price", 10, 2);
            table.Double("latitude");
        });

        var bp = Schema.Operations.Single().Blueprint!;

        var level = bp.Columns.First(c => c.Name == "level");
        Assert.Equal(ColumnType.SmallInteger, level.Type);

        var price = bp.Columns.First(c => c.Name == "price");
        Assert.Equal(ColumnType.Decimal, price.Type);
        Assert.Equal(10, price.Precision);
        Assert.Equal(2, price.Scale);

        var lat = bp.Columns.First(c => c.Name == "latitude");
        Assert.Equal(ColumnType.Double, lat.Type);
    }

    [Fact]
    public void New_misc_types_are_set_correctly()
    {
        Schema.Create("files", table =>
        {
            table.Uuid("public_id");
            table.Json("settings");
            table.Jsonb("metadata");
            table.Binary("content");
            table.Time("opens_at");
        });

        var bp = Schema.Operations.Single().Blueprint!;

        Assert.Equal(ColumnType.Uuid,   bp.Columns.First(c => c.Name == "public_id").Type);
        Assert.Equal(ColumnType.Json,   bp.Columns.First(c => c.Name == "settings").Type);
        Assert.Equal(ColumnType.Jsonb,  bp.Columns.First(c => c.Name == "metadata").Type);
        Assert.Equal(ColumnType.Binary, bp.Columns.First(c => c.Name == "content").Type);
        Assert.Equal(ColumnType.Time,   bp.Columns.First(c => c.Name == "opens_at").Type);
    }

    [Fact]
    public void Decimal_defaults_to_8_2_when_no_precision_given()
    {
        Schema.Create("orders", table => table.Decimal("total"));

        var col = Schema.Operations.Single().Blueprint!.Columns.Single();
        Assert.Equal(ColumnType.Decimal, col.Type);
        Assert.Equal(8, col.Precision);
        Assert.Equal(2, col.Scale);
    }

    [Fact]
    public void SoftDeletes_adds_nullable_deleted_at()
    {
        Schema.Create("posts", table =>
        {
            table.Id();
            table.SoftDeletes();
        });

        var bp = Schema.Operations.Single().Blueprint!;
        var col = bp.Columns.First(c => c.Name == "deleted_at");

        Assert.Equal(ColumnType.Timestamp, col.Type);
        Assert.True(col.IsNullable);
    }

    [Fact]
    public void RememberToken_adds_nullable_varchar100()
    {
        Schema.Create("users", table =>
        {
            table.Id();
            table.RememberToken();
        });

        var bp = Schema.Operations.Single().Blueprint!;
        var col = bp.Columns.First(c => c.Name == "remember_token");

        Assert.Equal(ColumnType.String, col.Type);
        Assert.Equal(100, col.Length);
        Assert.True(col.IsNullable);
    }

    [Fact]
    public void Timestamps_uses_expression_default()
    {
        Schema.Create("events", table => table.Timestamps());

        var bp = Schema.Operations.Single().Blueprint!;

        var createdAt = bp.Columns.First(c => c.Name == "created_at");
        Assert.True(createdAt.HasDefault);
        Assert.True(createdAt.IsDefaultExpression);
        Assert.Equal("NOW()", createdAt.DefaultValue);

        var updatedAt = bp.Columns.First(c => c.Name == "updated_at");
        Assert.True(updatedAt.HasDefault);
        Assert.True(updatedAt.IsDefaultExpression);
    }

    [Fact]
    public void DefaultExpression_stores_raw_sql()
    {
        Schema.Create("users", table =>
        {
            table.Uuid("id").DefaultExpression("gen_random_uuid()");
        });

        var col = Schema.Operations.Single().Blueprint!.Columns.Single();
        Assert.True(col.HasDefault);
        Assert.True(col.IsDefaultExpression);
        Assert.Equal("gen_random_uuid()", col.DefaultValue);
    }

    [Fact]
    public void GeneratedUuid_is_shorthand_for_expression_default()
    {
        Schema.Create("users", table =>
        {
            table.Uuid("public_id").GeneratedUuid();
        });

        var col = Schema.Operations.Single().Blueprint!.Columns.Single();
        Assert.True(col.IsDefaultExpression);
        Assert.Equal("gen_random_uuid()", col.DefaultValue);
    }

    [Fact]
    public void Polymorphic_adds_id_type_and_composite_index()
    {
        Schema.Create("comments", table =>
        {
            table.Id();
            table.Polymorphic("commentable");
        });

        var bp = Schema.Operations.Single().Blueprint!;

        var id = bp.Columns.First(c => c.Name == "commentable_id");
        Assert.Equal(ColumnType.BigInteger, id.Type);
        Assert.False(id.IsNullable);

        var type = bp.Columns.First(c => c.Name == "commentable_type");
        Assert.Equal(ColumnType.String, type.Type);
        Assert.False(type.IsNullable);

        var idx = bp.Indexes.Single();
        Assert.False(idx.IsUnique);
        Assert.Contains("commentable_id", idx.Columns);
        Assert.Contains("commentable_type", idx.Columns);
    }

    [Fact]
    public void NullablePolymorphic_adds_nullable_columns()
    {
        Schema.Create("tags", table =>
        {
            table.Id();
            table.NullablePolymorphic("taggable");
        });

        var bp = Schema.Operations.Single().Blueprint!;

        var id = bp.Columns.First(c => c.Name == "taggable_id");
        Assert.True(id.IsNullable);

        var type = bp.Columns.First(c => c.Name == "taggable_type");
        Assert.True(type.IsNullable);
    }

    [Fact]
    public void ColumnBuilder_Unique_adds_single_column_index()
    {
        Schema.Create("users", table =>
        {
            table.String("email").Unique();
        });

        var bp = Schema.Operations.Single().Blueprint!;
        var idx = bp.Indexes.Single();

        Assert.True(idx.IsUnique);
        Assert.Equal(["email"], idx.Columns);
    }
}
