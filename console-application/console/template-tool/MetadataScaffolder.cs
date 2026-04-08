namespace TemplateTool;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Serilog;
using Spectre.Console;
using System.Text;

public static class MetadataScaffolder
{
    private static readonly string OutputFile = Path.Combine(FindProjectRoot(), "Constants.cs");

    public static void Scaffold(IOrganizationService service, ILogger logger)
    {
        AnsiConsole.Status().Start("Fetching entity metadata...", ctx =>
        {
            var response = (RetrieveAllEntitiesResponse)service.Execute(
                new RetrieveAllEntitiesRequest { EntityFilters = EntityFilters.Attributes });

            var entities = response.EntityMetadata;
            logger.Information("Retrieved {Count} entities", entities.Length);
            ctx.Status($"Generating {entities.Length} classes...");

            var sb = new StringBuilder();
            sb.AppendLine("namespace Constants;");
            sb.AppendLine();

            foreach (var entity in entities)
            {
                AppendEntityClass(sb, entity);
            }

            File.WriteAllText(OutputFile, sb.ToString());
            logger.Information("Scaffolded {Count} entity classes into {File}", entities.Length, OutputFile);
        });

        AnsiConsole.MarkupLine($"[green]✓ Entity classes generated at {Markup.Escape(OutputFile)}[/]");
    }

    private static void AppendEntityClass(StringBuilder sb, EntityMetadata entity)
    {
        var className = ToPascalCase(entity.LogicalName);
        if (string.IsNullOrEmpty(className) || !char.IsLetter(className[0])) return;

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
        sb.AppendLine();
    }

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return AppContext.BaseDirectory;
    }

    private static string ToPascalCase(string logicalName) =>
        string.Concat(logicalName.Split('_')
            .Where(p => p.Length > 0)
            .Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
}
