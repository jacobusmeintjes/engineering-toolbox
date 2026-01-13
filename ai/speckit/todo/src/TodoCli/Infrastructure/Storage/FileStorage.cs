namespace TodoCli.Infrastructure.Storage;

/// <summary>
/// Implements atomic file operations with backup and recovery mechanisms
/// </summary>
public class FileStorage : IFileStorage
{
    public async Task<string> ReadAllTextAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            // Attempt recovery from backup
            var backupPath = $"{path}.bak";
            if (File.Exists(backupPath))
            {
                Console.WriteLine("⚠ Corruption detected. Restoring from backup...");
                RestoreFromBackup(path);
                return await File.ReadAllTextAsync(path);
            }

            throw;
        }
    }

    public async Task WriteAllTextAsync(string path, string contents)
    {
        // Create backup before write
        if (File.Exists(path))
        {
            CreateBackup(path);
        }

        // Atomic write pattern: write to temp, then rename
        var tempPath = $"{path}.tmp";
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            EnsureDirectoryExists(directory);
        }

        try
        {
            // Write to temp file
            await File.WriteAllTextAsync(tempPath, contents);

            // Atomic rename (overwrites existing file)
            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            // Clean up temp file on failure
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            throw;
        }
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public void CreateBackup(string path)
    {
        if (!File.Exists(path))
            return;

        var backupPath = $"{path}.bak";
        File.Copy(path, backupPath, overwrite: true);
    }

    public void RestoreFromBackup(string path)
    {
        var backupPath = $"{path}.bak";

        if (!File.Exists(backupPath))
            throw new FileNotFoundException($"Backup file not found: {backupPath}");

        File.Copy(backupPath, path, overwrite: true);
    }
}
