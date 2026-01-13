using TodoCli.Infrastructure.Configuration;
using TodoCli.Infrastructure.Storage;
using TodoCli.Models;

namespace TodoCli.Specs.Drivers;

/// <summary>
/// Test automation driver for task storage management
/// </summary>
public class TaskStorageDriver
{
    private readonly IFileStorage _fileStorage;
    private readonly StoragePathProvider _pathProvider;
    private readonly List<string> _testFilePaths = new();

    public TaskStorageDriver(IFileStorage fileStorage, StoragePathProvider pathProvider)
    {
        _fileStorage = fileStorage;
        _pathProvider = pathProvider;
    }

    public void CleanupTestData()
    {
        var filePath = _pathProvider.GetTasksFilePath();

        if (_fileStorage.FileExists(filePath))
        {
            File.Delete(filePath);
        }

        var backupPath = $"{filePath}.bak";
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }
    }

    public async Task SeedTasksAsync(List<TodoTask> tasks)
    {
        var repository = new JsonTaskRepository(_fileStorage, _pathProvider);
        await repository.SaveAllAsync(tasks);
    }
}
