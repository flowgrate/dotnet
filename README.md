# flowgrate/dotnet

C# SDK for [Flowgrate](https://github.com/flowgrate/core) — Laravel-style database migrations with a fluent API.

## How it works

Define migrations in C# using the fluent Blueprint API. The SDK serializes them to JSON and pipes to the Flowgrate CLI, which compiles and executes the SQL.

## Requirements

- [Flowgrate CLI](https://github.com/flowgrate/core/releases) — download the binary for your platform and put it on your `PATH`

```bash
# Linux (amd64)
curl -L https://github.com/flowgrate/core/releases/latest/download/flowgrate-linux-amd64 -o flowgrate
chmod +x flowgrate
sudo mv flowgrate /usr/local/bin/

# macOS (Apple Silicon)
curl -L https://github.com/flowgrate/core/releases/latest/download/flowgrate-darwin-arm64 -o flowgrate
chmod +x flowgrate
sudo mv flowgrate /usr/local/bin/

# macOS (Intel)
curl -L https://github.com/flowgrate/core/releases/latest/download/flowgrate-darwin-amd64 -o flowgrate
chmod +x flowgrate
sudo mv flowgrate /usr/local/bin/

# Or build from source
go install github.com/flowgrate/core@latest
```

## Setup

**1. Reference the SDK in your migrations project:**

```xml
<ProjectReference Include="path/to/Flowgrate/Flowgrate.csproj" />
```

**2. Add `Program.cs`:**

```csharp
using System.Reflection;
using Flowgrate;

FlowgrateRunner.Run(Assembly.GetExecutingAssembly());
```

**3. Create `flowgrate.yml` next to your project:**

Generate it with the CLI (recommended):

```bash
flowgrate init --db=postgres://user:pass@localhost/mydb
```

Or create manually:

```yaml
database:
  url: postgres://user:pass@localhost/mydb

migrations:
  project: ./Migrations
  sdk: csharp
```

**4. Generate and run migrations:**

```bash
flowgrate make CreateUsersTable
flowgrate up
```

## Migration anatomy

```csharp
using Flowgrate;

public class _20260402_163107_CreateUsersTable : Migration
{
    public static string Version => "20260402_163107";

    public override void Up()
    {
        Schema.Create("users", table =>
        {
            table.Id();
            table.String("name");
            table.String("email", 100);
            table.Timestamps();
        });
    }

    public override void Down()
    {
        Schema.DropIfExists("users");
    }
}
```

## Blueprint API reference

### Create / drop table

```csharp
Schema.Create("users", table => { ... });
Schema.Table("users", table => { ... });   // ALTER TABLE
Schema.Drop("users");
Schema.DropIfExists("users");
```

### Column types

```csharp
table.Id()                          // BIGSERIAL PRIMARY KEY
table.SmallInteger("level")         // SMALLINT
table.Integer("views")              // INTEGER
table.BigInteger("score")           // BIGINT
table.Decimal("price", 10, 2)       // NUMERIC(10, 2)
table.Float("rating")               // REAL
table.Double("latitude")            // DOUBLE PRECISION
table.Boolean("active")             // BOOLEAN
table.String("name")                // VARCHAR(255)
table.String("code", 10)            // VARCHAR(10)
table.Text("bio")                   // TEXT
table.Uuid("public_id")             // UUID
table.Json("settings")              // JSON
table.Jsonb("metadata")             // JSONB
table.Binary("avatar")              // BYTEA
table.Date("birthday")              // DATE
table.Time("opens_at")              // TIME
table.Timestamp("verified_at")      // TIMESTAMP
```

### Column modifiers (chainable)

```csharp
.Nullable()                         // NULL
.Default(value)                     // DEFAULT 'value'
.DefaultExpression("NOW()")         // DEFAULT NOW()  — raw SQL
.GeneratedUuid()                    // DEFAULT gen_random_uuid()
.Comment("description")
.Unique()                           // single-column unique index
```

### Helpers

```csharp
table.Timestamps()                  // created_at + updated_at TIMESTAMP DEFAULT NOW()
table.SoftDeletes()                 // deleted_at TIMESTAMP NULL
table.RememberToken()               // remember_token VARCHAR(100) NULL
table.Polymorphic("commentable")    // commentable_id BIGINT + commentable_type VARCHAR(255) + index
table.NullablePolymorphic("taggable")
```

### Foreign keys

```csharp
table.ForeignId("role_id")
     .Constrained("roles")          // REFERENCES roles(id)
     .OnDelete("cascade")
     .OnUpdate("cascade");
```

### Indexes

```csharp
table.Unique("email", "tenant_id").Name("uq_users_email_tenant");
table.Index("created_at");
table.Index("email", "name").Name("idx_users_search");
```

### ALTER TABLE

```csharp
Schema.Table("users", table =>
{
    table.AddColumn("phone").String(20).Nullable();
    table.ChangeColumn("name").String(500);
    table.DropColumn("avatar");
});
```

## Running in Docker

```bash
docker compose exec sdk dotnet run --project /migrations | flowgrate up
```
