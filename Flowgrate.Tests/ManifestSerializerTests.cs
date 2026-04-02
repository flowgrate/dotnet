using System.Text.Json;
using Flowgrate;

namespace Flowgrate.Tests;

public class ManifestSerializerTests
{
    private class CreateUsersTableMigration : Migration
    {
        public override void Up()
        {
            Schema.Create("users", table =>
            {
                table.Id();
                table.String("name");
                table.String("email");
                table.Boolean("active").Default(true).Comment("Статус пользователя");
                table.ForeignId("role_id").Constrained("roles").OnDelete("cascade");
                table.Timestamps();
                table.Unique("email").Name("uq_users_email");
                table.Index("role_id");
            });
        }

        public override void Down()
        {
            Schema.DropIfExists("users");
        }
    }

    private class AlterTableMigration : Migration
    {
        public override void Up()
        {
            Schema.Table("users", table =>
            {
                table.AddColumn("status").String(50).Default("active");
                table.ChangeColumn("name").String(500).Nullable();
                table.DropColumn("email");
            });
        }

        public override void Down()
        {
            Schema.Table("users", table =>
            {
                table.DropColumn("status");
                table.ChangeColumn("name").String(255);
                table.AddColumn("email").String();
            });
        }
    }

    [Fact]
    public void Build_create_table_manifest()
    {
        var manifest = ManifestSerializer.Build(new CreateUsersTableMigration(), "20240101_120000_CreateUsersTable");

        Assert.Equal("20240101_120000_CreateUsersTable", manifest.Migration);

        // up
        var up = Assert.Single(manifest.Up);
        Assert.Equal("create_table", up.Action);
        Assert.Equal("users", up.Table);
        Assert.Equal(7, up.Columns!.Count); // id, name, email, active, role_id, created_at, updated_at

        var id = up.Columns.First(c => c.Name == "id");
        Assert.Equal("big_integer", id.Type);
        Assert.True(id.Primary);
        Assert.True(id.AutoIncrement);

        var active = up.Columns.First(c => c.Name == "active");
        Assert.Equal("boolean", active.Type);
        Assert.True((bool)active.Default!);
        Assert.Equal("Статус пользователя", active.Comment);

        Assert.Equal(2, up.Indexes!.Count);
        Assert.True(up.Indexes[0].Unique);
        Assert.Equal("uq_users_email", up.Indexes[0].Name);

        var fk = Assert.Single(up.ForeignKeys!);
        Assert.Equal("role_id", fk.Column);
        Assert.Equal("roles", fk.ReferencesTable);
        Assert.Equal("cascade", fk.OnDelete);

        // down
        var down = Assert.Single(manifest.Down);
        Assert.Equal("drop_table", down.Action);
        Assert.True(down.IfExists);
    }

    [Fact]
    public void Build_alter_table_manifest()
    {
        var manifest = ManifestSerializer.Build(new AlterTableMigration(), "20240201_090000_AlterTableMigration");

        var up = Assert.Single(manifest.Up);
        Assert.Equal("alter_table", up.Action);
        Assert.Equal(3, up.Columns!.Count);

        var add = up.Columns.First(c => c.Name == "status");
        Assert.Equal("add", add.ColumnAction);
        Assert.Equal("string", add.Type);

        var change = up.Columns.First(c => c.Name == "name");
        Assert.Equal("change", change.ColumnAction);
        Assert.Equal(500, change.Length);

        var drop = up.Columns.First(c => c.Name == "email");
        Assert.Equal("drop", drop.ColumnAction);
    }

    [Fact]
    public void Serialize_produces_valid_json()
    {
        var manifest = ManifestSerializer.Build(new CreateUsersTableMigration(), "20240101_120000_CreateUsersTable");
        var json = ManifestSerializer.Serialize(manifest);

        var doc = JsonDocument.Parse(json);
        Assert.Equal("20240101_120000_CreateUsersTable", doc.RootElement.GetProperty("migration").GetString());
        Assert.Equal("create_table", doc.RootElement.GetProperty("up")[0].GetProperty("action").GetString());
    }
}
