namespace TemplateTool;

/// <summary>
/// Lightweight console styling via ANSI escape codes.
/// Drop-in replacement for Spectre.Console across all template apps.
/// </summary>
public static class Ansi
{
    // ── Reset ────────────────────────────────────────────────────────────────
    private const string Reset = "\x1b[0m";

    // ── Styles ───────────────────────────────────────────────────────────────
    private const string Bold  = "\x1b[1m";
    private const string Dim   = "\x1b[2m";

    // ── Foreground colors ────────────────────────────────────────────────────
    private const string FgRed     = "\x1b[91m";
    private const string FgGreen   = "\x1b[92m";
    private const string FgYellow  = "\x1b[93m";
    private const string FgCyan    = "\x1b[96m";
    private const string FgWhite   = "\x1b[97m";
    private const string FgGray    = "\x1b[90m";

    // ── Public helpers — colors ──────────────────────────────────────────────
    public static string Red(string text)    => $"{FgRed}{text}{Reset}";
    public static string Green(string text)  => $"{FgGreen}{text}{Reset}";
    public static string Yellow(string text) => $"{FgYellow}{text}{Reset}";
    public static string Cyan(string text)   => $"{FgCyan}{text}{Reset}";
    public static string White(string text)  => $"{FgWhite}{text}{Reset}";
    public static string Gray(string text)   => $"{FgGray}{text}{Reset}";

    // ── Public helpers — style combos ────────────────────────────────────────
    public static string BoldRed(string text)    => $"{Bold}{FgRed}{text}{Reset}";
    public static string BoldGreen(string text)  => $"{Bold}{FgGreen}{text}{Reset}";
    public static string BoldCyan(string text)   => $"{Bold}{FgCyan}{text}{Reset}";
    public static string BoldWhite(string text)  => $"{Bold}{FgWhite}{text}{Reset}";
    public static string BoldYellow(string text) => $"{Bold}{FgYellow}{text}{Reset}";
    public static string DimGray(string text)    => $"{Dim}{FgGray}{text}{Reset}";

    // ── Semantic shortcuts ───────────────────────────────────────────────────
    public static string Success(string text) => $"{Bold}{FgGreen}✓ {text}{Reset}";
    public static string Error(string text)   => $"{Bold}{FgRed}✗ {text}{Reset}";
    public static string Info(string text)    => $"{FgCyan}ℹ {text}{Reset}";
    public static string Warn(string text)    => $"{FgYellow}⚠ {text}{Reset}";

    // ── Box drawing ──────────────────────────────────────────────────────────
    /// <summary>
    /// Draws a rounded-corner box around a message.
    /// Optionally shows a header label above the top border.
    /// </summary>
    public static void WriteBox(string message, string? header = null)
    {
        var lines   = message.Split('\n');
        int inner   = lines.Max(l => l.Length);
        int width   = Math.Max(inner, (header?.Length ?? 0) + 2) + 2;

        string top    = $"╭{Repeat('─', width)}╮";
        string bottom = $"╰{Repeat('─', width)}╯";

        if (header is not null)
        {
            int pad = width - header.Length - 2;
            top = $"╭─ {BoldCyan(header)} {Repeat('─', Math.Max(0, pad))}╮";
        }

        Console.WriteLine(top);
        foreach (var line in lines)
            Console.WriteLine($"│ {line.PadRight(width - 2)} │");
        Console.WriteLine(bottom);
    }

    /// <summary>
    /// Prints a large block-letter banner for the application title.
    /// Uses simple block characters — no external dependency.
    /// </summary>
    public static void WriteBanner(string title)
    {
        Console.WriteLine();
        string bar = Repeat('━', title.Length + 6);
        Console.WriteLine($"  {FgCyan}{Bold}{bar}{Reset}");
        Console.WriteLine($"  {FgCyan}{Bold}┃  {FgWhite}{title}{FgCyan}  ┃{Reset}");
        Console.WriteLine($"  {FgCyan}{Bold}{bar}{Reset}");
        Console.WriteLine();
    }

    /// <summary>
    /// Prints a horizontal divider line.
    /// </summary>
    public static void WriteDivider(int length = 50)
        => Console.WriteLine(DimGray(Repeat('─', length)));

    // ── Internal ─────────────────────────────────────────────────────────────
    private static string Repeat(char c, int count)
        => new(c, Math.Max(0, count));
}
