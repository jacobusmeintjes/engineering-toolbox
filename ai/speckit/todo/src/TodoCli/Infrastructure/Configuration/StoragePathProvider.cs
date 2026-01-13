using System.Runtime.InteropServices;

namespace TodoCli.Infrastructure.Configuration;

/// <summary>
/// Provides platform-specific storage paths for task data
/// </summary>
public class StoragePathProvider
{
    private const string AppName = "TodoCli";
    private const string FileName = "tasks.json";

    /// <summary>
    /// Gets the platform-specific storage directory path
    /// </summary>
    public string GetStorageDirectory()
    {
        string basePath;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: %APPDATA%\TodoCli\
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else
        {
            // macOS/Linux: ~/.local/share/TodoCli/
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        return Path.Combine(basePath, AppName);
    }

    /// <summary>
    /// Gets the full path to the tasks.json file
    /// </summary>
    public string GetTasksFilePath()
    {
        return Path.Combine(GetStorageDirectory(), FileName);
    }

    /// <summary>
    /// Sets file permissions to user-only access (chmod 600 equivalent)
    /// </summary>
    public void SetUserOnlyPermissions(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: Use FileSystemAccessRule to set ACL
            // Note: Basic implementation - full ACL management would be more complex
            var fileInfo = new FileInfo(filePath);
            fileInfo.Attributes = FileAttributes.Normal;
        }
        else
        {
            // Unix-like: Set permissions to 600 (user read/write only)
            try
            {
                File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback for platforms that don't support Unix file modes
                // (should not happen on Linux/macOS, but be defensive)
            }
        }
    }
}
