using Serilog;
using Spectre.Console;
using TemplateTool;

var (service, logger) = AppRunner.Connect();
if (service is null) return;

// ── Your business logic here ──────────────────────────────────────────────────
AnsiConsole.Write("\n\nThis is your program\n\n");

Log.CloseAndFlush();
service.Dispose();