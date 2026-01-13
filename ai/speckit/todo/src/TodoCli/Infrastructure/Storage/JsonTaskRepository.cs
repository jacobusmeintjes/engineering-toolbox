using System.Text.Json;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Models;
using TodoCli.Services;

namespace TodoCli.Infrastructure.Storage;

/// <summary>
/// JSON file-based implementation of task persistence
/// </summary>
public class JsonTaskRepository : ITaskRepository
{
    private readonly IFileStorage _fileStorage;
    private readonly StoragePathProvider _pathProvider;

    public JsonTaskRepository(IFileStorage fileStorage, StoragePathProvider pathProvider)
    {
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    public async Task<List<TodoTask>> GetAllAsync()
    {
        var filePath = _pathProvider.GetTasksFilePath();
        var directory = _pathProvider.GetStorageDirectory();

        // Ensure directory exists
        _fileStorage.EnsureDirectoryExists(directory);

        // If file doesn't exist, return empty list and create it
        if (!_fileStorage.FileExists(filePath))
        {
            var emptyList = new List<TodoTask>();
            await SaveAllAsync(emptyList);
            return emptyList;
        }

        try
        {
            var json = await _fileStorage.ReadAllTextAsync(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<TodoTask>();
            }

            var tasks = JsonSerializer.Deserialize(json, TodoTaskJsonContext.Default.ListTodoTask);
            return tasks ?? new List<TodoTask>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse tasks file. The file may be corrupted: {ex.Message}", ex);
        }
    }

    public async Task SaveAllAsync(List<TodoTask> tasks)
    {
        if (tasks == null)
            throw new ArgumentNullException(nameof(tasks));

        var filePath = _pathProvider.GetTasksFilePath();
        var directory = _pathProvider.GetStorageDirectory();

        // Ensure directory exists
        _fileStorage.EnsureDirectoryExists(directory);

        // Serialize with indentation for human readability
        var json = JsonSerializer.Serialize(tasks, TodoTaskJsonContext.Default.ListTodoTask);

        // Atomic write with backup
        await _fileStorage.WriteAllTextAsync(filePath, json);

        // Set user-only permissions
        _pathProvider.SetUserOnlyPermissions(filePath);
    }

    public async Task<TodoTask?> GetByIdAsync(string idOrPartial)
    {
        if (string.IsNullOrWhiteSpace(idOrPartial))
            throw new ArgumentException("ID cannot be empty", nameof(idOrPartial));

        var tasks = await GetAllAsync();

        // Try exact GUID match first
        if (Guid.TryParse(idOrPartial, out var exactId))
        {
            var exactMatch = tasks.FirstOrDefault(t => t.Id == exactId);
            if (exactMatch != null)
                return exactMatch;
        }

        // Try partial ID match (minimum 4 characters)
        if (idOrPartial.Length < 4)
            throw new ArgumentException("Partial ID must be at least 4 characters", nameof(idOrPartial));

        var matches = tasks.Where(t => t.Id.ToString().StartsWith(idOrPartial, StringComparison.OrdinalIgnoreCase)).ToList();

        return matches.Count switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException($"Ambiguous ID '{idOrPartial}' matches {matches.Count} tasks. Use a longer prefix.")
        };
    }

    public async Task<List<TodoTask>> FindByPartialIdAsync(string partialId)
    {
        if (string.IsNullOrWhiteSpace(partialId))
            throw new ArgumentException("Partial ID cannot be empty", nameof(partialId));

        var tasks = await GetAllAsync();

        return tasks
            .Where(t => t.Id.ToString().StartsWith(partialId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
