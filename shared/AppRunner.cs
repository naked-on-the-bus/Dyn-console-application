namespace TemplateTool;

using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Serilog;
using Spectre.Console;

public static class AppRunner
{
    public static (ServiceClient? Service, ILogger Logger) Connect()
    {
        // ── 1. Configuration ──────────────────────────────────────────────────────────
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yaml", optional: false, reloadOnChange: false)
            .Build();

        var dynamics = configuration.GetSection("Dynamics");

        // ── 2. Connection String ──────────────────────────────────────────────────────
        var connectionString = ConnectionBuilder.Build(
            dynamics["AuthType"]!,
            dynamics["Url"]!,
            dynamics["AppId"]!,
            dynamics["RedirectUri"]!,
            dynamics["LoginPrompt"]!
        );

        // ── 3. Logging ────────────────────────────────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // ── 4. Welcome Banner ────────────────────────────────────────────────────────
        AnsiConsole.Write(new FigletText("Console Runner").Color(Color.Cyan1));
        AnsiConsole.Write(
            new Panel("[bold green]A template for console-application / By naked-on-the-bus[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 0));

        // ── 5. Connect ────────────────────────────────────────────────────────────────
        Log.Information("Connecting to {Url}...", dynamics["Url"]);
        var service = new ServiceClient(connectionString);

        // ── 6. Verify Connection ─────────────────────────────────────────────────────
        if (!service.IsReady)
        {
            Log.Error("Connection failed: {Error}", service.LastError);
            AnsiConsole.MarkupLine($"[bold red]✗ Connection failed:[/] {Markup.Escape(service.LastError)}");
            Log.CloseAndFlush();
            service.Dispose();
            return (null, Log.Logger);
        }

        Log.Information("Connected to {Url}", service.ConnectedOrgUriActual);
        AnsiConsole.Write(
            new Panel($"[bold green]✓ Connected to:[/] {Markup.Escape(service.ConnectedOrgUriActual.ToString())}")
                .Border(BoxBorder.Rounded)
                .Header("[green]Connection Status[/]"));

        return (service, Log.Logger);
    }
}
