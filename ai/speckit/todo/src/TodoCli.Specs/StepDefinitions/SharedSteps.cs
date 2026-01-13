using TechTalk.SpecFlow;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Infrastructure.Storage;
using TodoCli.Services;
using TestUtilities;

namespace TodoCli.Specs.StepDefinitions;

[Binding]
public class SharedSteps : IDisposable
{
    private readonly TestFileSystem _testFileSystem;
    private readonly IFileStorage _fileStorage;
    private readonly StoragePathProvider _pathProvider;
    private readonly ITaskRepository _taskRepository;

    public SharedSteps()
    {
        _testFileSystem = new TestFileSystem();
        _fileStorage = new FileStorage();
        _pathProvider = new TestStoragePathProvider(_testFileSystem.TempDirectory);
        _taskRepository = new JsonTaskRepository(_fileStorage, _pathProvider);

        Environment.SetEnvironmentVariable("TEST_MODE", "1");
    }

    [Given(@"I have an empty task list")]
    public async Task GivenIHaveAnEmptyTaskList()
    {
        var tasksFile = _pathProvider.GetTasksFilePath();
        if (File.Exists(tasksFile))
        {
            File.Delete(tasksFile);
        }
    }

    public void Dispose()
    {
        _testFileSystem.Dispose();
    }

    private class TestStoragePathProvider : StoragePathProvider
    {
        private readonly string _testDirectory;

        public TestStoragePathProvider(string testDirectory)
        {
            _testDirectory = testDirectory;
        }

        public new string GetStorageDirectory() => _testDirectory;
        public new string GetTasksFilePath() => Path.Combine(_testDirectory, "tasks.json");
    }
}
