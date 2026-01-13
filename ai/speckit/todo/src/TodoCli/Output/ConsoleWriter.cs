namespace TodoCli.Output;

/// <summary>
/// Console output implementation with color support detection
/// </summary>
public class ConsoleWriter : IConsoleWriter
{
    private readonly ColorProvider _colorProvider;

    public ConsoleWriter(ColorProvider colorProvider)
    {
        _colorProvider = colorProvider ?? throw new ArgumentNullException(nameof(colorProvider));
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteSuccess(string message)
    {
        Console.WriteLine(_colorProvider.Green($"✓ {message}"));
    }

    public void WriteError(string message)
    {
        Console.WriteLine(_colorProvider.Red($"✗ Error: {message}"));
    }

    public void WriteWarning(string message)
    {
        Console.WriteLine(_colorProvider.Yellow($"⚠ {message}"));
    }

    public void WriteInfo(string message)
    {
        Console.WriteLine(_colorProvider.Gray($"ℹ {message}"));
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }
}
