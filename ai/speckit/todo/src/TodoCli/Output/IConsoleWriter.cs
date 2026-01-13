namespace TodoCli.Output;

/// <summary>
/// Abstraction for console output (enables testing)
/// </summary>
public interface IConsoleWriter
{
    void WriteLine(string message);
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteWarning(string message);
    void WriteInfo(string message);
    string? ReadLine();
}
