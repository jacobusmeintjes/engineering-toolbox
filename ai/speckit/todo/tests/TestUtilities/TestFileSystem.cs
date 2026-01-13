namespace TestUtilities;

/// <summary>
/// Helper for managing temporary test directories and files
/// </summary>
public class TestFileSystem : IDisposable
{
    private readonly string _tempDirectory;

    public TestFileSystem()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"TodoCliTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public string TempDirectory => _tempDirectory;

    public string CreateTempFile(string fileName, string content = "")
    {
        var filePath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
