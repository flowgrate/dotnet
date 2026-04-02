using System.Reflection;

namespace Flowgrate;

public static class FlowgrateRunner
{
    public static void Run(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var migrations = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsAbstract)
            .Select(t => new
            {
                Type = t,
                Version = GetVersion(t),
            })
            .Where(m => m.Version != null)
            .OrderBy(m => m.Version)
            .ToList();

        foreach (var m in migrations)
        {
            var migrationName = $"{m.Version}_{m.Type.Name}";
            var migration = (Migration)Activator.CreateInstance(m.Type)!;
            var manifest = ManifestSerializer.Build(migration, migrationName);
            Console.WriteLine(ManifestSerializer.Serialize(manifest));
        }
    }

    private static string? GetVersion(Type t)
    {
        var prop = t.GetProperty("Version", BindingFlags.Static | BindingFlags.Public);
        return prop?.GetValue(null) as string;
    }

}
