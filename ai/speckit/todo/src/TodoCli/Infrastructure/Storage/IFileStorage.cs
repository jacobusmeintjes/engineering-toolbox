namespace TodoCli.Infrastructure.Storage;

/// <summary>
/// Abstraction for atomic file operations with backup and recovery
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Reads all text from a file asynchronously
    /// </summary>
    /// <param name="path">Full path to the file</param>
    /// <returns>File contents as string</returns>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes text to a file atomically (temp file + rename pattern)
    /// </summary>
    /// <param name="path">Full path to the file</param>
    /// <param name="contents">Text content to write</param>
    Task WriteAllTextAsync(string path, string contents);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="path">Full path to the file</param>
    /// <returns>True if file exists</returns>
    bool FileExists(string path);

    /// <summary>
    /// Creates a directory if it doesn't exist
    /// </summary>
    /// <param name="path">Full path to the directory</param>
    void EnsureDirectoryExists(string path);

    /// <summary>
    /// Creates a backup of a file (copy to .bak)
    /// </summary>
    /// <param name="path">Full path to the file to backup</param>
    void CreateBackup(string path);

    /// <summary>
    /// Restores a file from its backup
    /// </summary>
    /// <param name="path">Full path to the file to restore</param>
    void RestoreFromBackup(string path);
}
