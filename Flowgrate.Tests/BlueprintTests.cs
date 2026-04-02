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
}
