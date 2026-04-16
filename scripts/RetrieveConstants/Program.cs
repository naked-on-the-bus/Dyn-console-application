using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Text;

// ── 1. Locate config.yaml at repo root ──────────────────────────────────────
var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
var configPath = Path.Combine(repoRoot, "config.yaml");
if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"\x1b[1m\x1b[91m✗ config.yaml not found at repo root.\x1b[0m");
    Console.Error.WriteLine($"\x1b[90m  Expected: {configPath}\x1b[0m");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(repoRoot)
    .AddYamlFile("config.yaml", optional: false)
    .Build();

var dynamics = configuration.GetSection("Dynamics");

var connectionString =
    $"AuthType={dynamics["AuthType"]};Url={dynamics["Url"]};" +
    $"AppId={dynamics["AppId"]};RedirectUri={dynamics["RedirectUri"]};" +
    $"LoginPrompt={dynamics["LoginPrompt"]};";

// ── 2. Connect ──────────────────────────────────────────────────────────────
Console.WriteLine();
var bar = new string('━', "Retrieve Constants".Length + 6);
Console.WriteLine($"  \x1b[96m\x1b[1m{bar}\x1b[0m");
Console.WriteLine($"  \x1b[96m\x1b[1m┃  \x1b[97mRetrieve Constants\x1b[96m  ┃\x1b[0m");
Console.WriteLine($"  \x1b[96m\x1b[1m{bar}\x1b[0m");
Console.WriteLine();
Console.WriteLine($"\x1b[90mConnecting to {dynamics["Url"]}...\x1b[0m");

using var service = new ServiceClient(connectionString);

if (!service.IsReady)
{
    Console.Error.WriteLine($"\x1b[1m\x1b[91m✗ Connection failed:\x1b[0m {service.LastError}");
    return 1;
}

Console.WriteLine($"\x1b[92m✓ Connected to {service.ConnectedOrgUriActual}\x1b[0m");

// ── 3. Output directory (pass as argument or defaults to ./constants) ───────
var outputDir = args.Length > 0
    ? args[0]
    : Path.Combine(repoRoot, "constants");

Directory.CreateDirectory(outputDir);

// ── 4. Fetch metadata & generate classes ────────────────────────────────────
Console.WriteLine($"\x1b[90mFetching entity metadata...\x1b[0m");
{
    var response = (RetrieveAllEntitiesResponse)service.Execute(
        new RetrieveAllEntitiesRequest { EntityFilters = EntityFilters.Attributes });

    var entities = response.EntityMetadata;
    Console.WriteLine($"\x1b[90mGenerating {entities.Length} classes...\x1b[0m");

    var generated = 0;
    foreach (var entity in entities)
    {
        var className = ToPascalCase(entity.LogicalName);
        if (string.IsNullOrEmpty(className) || !char.IsLetter(className[0])) continue;

        var sb = new StringBuilder();
        sb.AppendLine("namespace Constants;");
        sb.AppendLine();
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public const string EntityLogicalName = \"{entity.LogicalName}\";");

        if (entity.Attributes is { Length: > 0 })
        {
            var seen = new HashSet<string> { "EntityLogicalName", className };
            foreach (var attr in entity.Attributes.OrderBy(a => a.LogicalName))
            {
                var fieldName = ToPascalCase(attr.LogicalName);
                if (string.IsNullOrEmpty(fieldName) || !char.IsLetter(fieldName[0])) continue;
                if (!seen.Add(fieldName)) continue;
                sb.AppendLine($"    public const string {fieldName} = \"{attr.LogicalName}\";");
            }
        }

        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(outputDir, $"{className}.cs"), sb.ToString());
        generated++;
    }

    Console.WriteLine($"\x1b[92m✓ Generated {generated} constant classes in {outputDir}\x1b[0m");
}

return 0;

// ── Helpers ─────────────────────────────────────────────────────────────────
static string ToPascalCase(string logicalName) =>
    string.Concat(logicalName.Split('_')
        .Where(p => p.Length > 0)
        .Select(p => char.ToUpperInvariant(p[0]) + p[1..]));

static string FindRepoRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "config.yaml")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return startDir;
}
