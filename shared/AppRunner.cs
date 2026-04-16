namespace TemplateTool;

using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Serilog;

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
        Ansi.WriteBanner("Console Runner");
        Ansi.WriteBox(Ansi.BoldGreen("A template for console-application"));
        Console.WriteLine();

        // ── 5. Connect ────────────────────────────────────────────────────────────────
        Log.Information("Connecting to {Url}...", dynamics["Url"]);
        Console.WriteLine(Ansi.Gray($"Connecting to {dynamics["Url"]}..."));
        var service = new ServiceClient(connectionString);

        // ── 6. Verify Connection ─────────────────────────────────────────────────────
        if (!service.IsReady)
        {
            Log.Error("Connection failed: {Error}", service.LastError);
            Console.WriteLine(Ansi.Error($"Connection failed: {service.LastError}"));
            Log.CloseAndFlush();
            service.Dispose();
            return (null, Log.Logger);
        }

        Log.Information("Connected to {Url}", service.ConnectedOrgUriActual);
        Ansi.WriteBox(
            Ansi.Success($"Connected to: {service.ConnectedOrgUriActual}"),
            "Connection Status");

        return (service, Log.Logger);
    }
}
