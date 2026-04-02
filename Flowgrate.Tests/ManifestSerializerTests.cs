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

    private class NewTypesMigration : Migration
    {
        public override void Up()
        {
            Schema.Create("products", table =>
            {
                table.Uuid("public_id").GeneratedUuid();
                table.SmallInteger("stock");
                table.Decimal("price", 10, 2);
                table.Double("weight");
                table.Json("attributes");
                table.Jsonb("metadata");
                table.Binary("thumbnail");
                table.Time("available_from");
                table.Polymorphic("imageable");
                table.SoftDeletes();
                table.RememberToken();
                table.Timestamps();
            });
        }

        public override void Down()
        {
            Schema.DropIfExists("products");
        }
    }

    // --- Existing tests ---

    [Fact]
    public void Build_create_table_manifest()
    {
        var manifest = ManifestSerializer.Build(new CreateUsersTableMigration(), "20240101_120000_CreateUsersTable");

        Assert.Equal("20240101_120000_CreateUsersTable", manifest.Migration);

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

    // --- New type serialization tests ---

    [Fact]
    public void New_types_serialize_to_correct_strings()
    {
        var manifest = ManifestSerializer.Build(new NewTypesMigration(), "20260402_000000_NewTypes");
        var up = Assert.Single(manifest.Up);
        var cols = up.Columns!.ToDictionary(c => c.Name);

        Assert.Equal("uuid",          cols["public_id"].Type);
        Assert.Equal("small_integer", cols["stock"].Type);
        Assert.Equal("decimal",       cols["price"].Type);
        Assert.Equal("double",        cols["weight"].Type);
        Assert.Equal("json",          cols["attributes"].Type);
        Assert.Equal("jsonb",         cols["metadata"].Type);
        Assert.Equal("binary",        cols["thumbnail"].Type);
        Assert.Equal("time",          cols["available_from"].Type);
        Assert.Equal("timestamp",     cols["deleted_at"].Type);
        Assert.Equal("string",        cols["remember_token"].Type);
    }

    [Fact]
    public void Decimal_serializes_precision_and_scale()
    {
        var manifest = ManifestSerializer.Build(new NewTypesMigration(), "20260402_000000_NewTypes");
        var price = manifest.Up[0].Columns!.First(c => c.Name == "price");

        Assert.Equal(10, price.Precision);
        Assert.Equal(2, price.Scale);
    }

    [Fact]
    public void DefaultExpression_goes_to_default_expression_field_not_default()
    {
        var manifest = ManifestSerializer.Build(new NewTypesMigration(), "20260402_000000_NewTypes");
        var col = manifest.Up[0].Columns!.First(c => c.Name == "public_id");

        Assert.Null(col.Default);
        Assert.Equal("gen_random_uuid()", col.DefaultExpression);
    }

    [Fact]
    public void Timestamps_serialize_as_expression_defaults()
    {
        var manifest = ManifestSerializer.Build(new NewTypesMigration(), "20260402_000000_NewTypes");
        var cols = manifest.Up[0].Columns!.ToDictionary(c => c.Name);

        Assert.Null(cols["created_at"].Default);
        Assert.Equal("NOW()", cols["created_at"].DefaultExpression);

        Assert.Null(cols["updated_at"].Default);
        Assert.Equal("NOW()", cols["updated_at"].DefaultExpression);
    }

    [Fact]
    public void Polymorphic_serializes_as_two_columns_and_index()
    {
        var manifest = ManifestSerializer.Build(new NewTypesMigration(), "20260402_000000_NewTypes");
        var up = manifest.Up[0];
        var cols = up.Columns!.ToDictionary(c => c.Name);

        Assert.Equal("big_integer", cols["imageable_id"].Type);
        Assert.False(cols["imageable_id"].Nullable);
        Assert.Equal("string", cols["imageable_type"].Type);
        Assert.False(cols["imageable_type"].Nullable);

        var idx = up.Indexes!.First(i => i.Columns.Contains("imageable_id"));
        Assert.False(idx.Unique);
        Assert.Contains("imageable_type", idx.Columns);
    }

    [Fact]
    public void Literal_Default_does_not_bleed_into_default_expression()
    {
        var manifest = ManifestSerializer.Build(new CreateUsersTableMigration(), "20240101_120000_CreateUsersTable");
        var active = manifest.Up[0].Columns!.First(c => c.Name == "active");

        Assert.NotNull(active.Default);
        Assert.Null(active.DefaultExpression);
    }
}
