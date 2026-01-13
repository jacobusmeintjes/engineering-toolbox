namespace TodoCli.Output;

/// <summary>
/// Provides ANSI color codes with fallback for non-supporting terminals
/// </summary>
public class ColorProvider
{
    private readonly bool _supportsColor;

    public ColorProvider()
    {
        // Detect color support (simplified - could be more sophisticated)
        _supportsColor = !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") == null;
    }

    public string Green(string text) => _supportsColor ? $"\x1b[32m{text}\x1b[0m" : text;
    public string Red(string text) => _supportsColor ? $"\x1b[31m{text}\x1b[0m" : text;
    public string Yellow(string text) => _supportsColor ? $"\x1b[33m{text}\x1b[0m" : text;
    public string Cyan(string text) => _supportsColor ? $"\x1b[36m{text}\x1b[0m" : text;
    public string Gray(string text) => _supportsColor ? $"\x1b[90m{text}\x1b[0m" : text;
    public string Bold(string text) => _supportsColor ? $"\x1b[1m{text}\x1b[0m" : text;
    public string Strikethrough(string text) => _supportsColor ? $"\x1b[9m{text}\x1b[0m" : text;

    public bool SupportsColor => _supportsColor;
}
