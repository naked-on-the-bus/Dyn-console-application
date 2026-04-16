using Serilog;
using TemplateTool;
using Microsoft.Xrm.Sdk;
using Query;

var (service, logger) = AppRunner.Connect();
if (service is null) return;

// ── Your business logic here ──────────────────────────────────────────────────
Console.WriteLine($"\n{Ansi.BoldCyan("Start")}\n");

Log.CloseAndFlush();
service.Dispose();