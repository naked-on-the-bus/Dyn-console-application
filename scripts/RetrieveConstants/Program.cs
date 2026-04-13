using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Spectre.Console;
using System.Text;

// ── 1. Locate config.yaml at repo root ──────────────────────────────────────
var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
var configPath = Path.Combine(repoRoot, "config.yaml");
if (!File.Exists(configPath))
{
    AnsiConsole.MarkupLine("[bold red]✗ config.yaml not found at repo root.[/]");
    AnsiConsole.MarkupLine($"[grey]Expected: {Markup.Escape(configPath)}[/]");
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
AnsiConsole.Write(new FigletText("Retrieve Constants").Color(Color.Cyan1));
AnsiConsole.MarkupLine($"[grey]Connecting to {Markup.Escape(dynamics["Url"]!)}...[/]");

using var service = new ServiceClient(connectionString);

if (!service.IsReady)
{
    AnsiConsole.MarkupLine($"[bold red]✗ Connection failed:[/] {Markup.Escape(service.LastError)}");
    return 1;
}

AnsiConsole.MarkupLine($"[green]✓ Connected to {Markup.Escape(service.ConnectedOrgUriActual.ToString())}[/]");

// ── 3. Output directory (pass as argument or defaults to ./constants) ───────
var outputDir = args.Length > 0
    ? args[0]
    : Path.Combine(repoRoot, "constants");

Directory.CreateDirectory(outputDir);

// ── 4. Fetch metadata & generate classes ────────────────────────────────────
AnsiConsole.Status().Start("Fetching entity metadata...", ctx =>
{
    var response = (RetrieveAllEntitiesResponse)service.Execute(
        new RetrieveAllEntitiesRequest { EntityFilters = EntityFilters.Attributes });

    var entities = response.EntityMetadata;
    ctx.Status($"Generating {entities.Length} classes...");

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

    AnsiConsole.MarkupLine($"[green]✓ Generated {generated} constant classes in {Markup.Escape(outputDir)}[/]");
});

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
