using Serilog;
using Spectre.Console;
using TemplateTool;
using Microsoft.Xrm.Sdk;
using Query;

var (service, logger) = AppRunner.Connect();
if (service is null) return;

// ── Your business logic here ──────────────────────────────────────────────────
AnsiConsole.Write("\n\nStart\n\n");

Log.CloseAndFlush();
service.Dispose();