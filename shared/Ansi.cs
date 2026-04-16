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
    public static string Success(string text) => $"{Bold}{FgGreen}+ {text}{Reset}";
    public static string Error(string text)   => $"{Bold}{FgRed}x {text}{Reset}";
    public static string Info(string text)    => $"{FgCyan}> {text}{Reset}";
    public static string Warn(string text)    => $"{FgYellow}! {text}{Reset}";

    // ── Layout ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Prints the application title in large, bold text with extra spacing.
    /// </summary>
    public static void WriteBanner(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"  {Bold}{FgCyan}{title.ToUpperInvariant()}{Reset}");
        //Console.WriteLine($"  {FgCyan}{new string('-', title.Length)}{Reset}");
    }

    /// <summary>
    /// Prints a horizontal divider line using plain ASCII dashes.
    /// </summary>
    public static void WriteDivider(int length = 50)
        => Console.WriteLine(DimGray(new string('-', length)));
}
