using BoDi;
using TechTalk.SpecFlow;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Infrastructure.Storage;
using TodoCli.Services;
using TodoCli.Specs.Drivers;

namespace TodoCli.Specs.Hooks;

[Binding]
public class TestHooks
{
    private readonly IObjectContainer _container;

    public TestHooks(IObjectContainer container)
    {
        _container = container;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        // Register dependencies for each scenario
        var fileStorage = new FileStorage();
        var pathProvider = new StoragePathProvider();
        var repository = new JsonTaskRepository(fileStorage, pathProvider);
        var taskService = new TaskService(repository);

        _container.RegisterInstanceAs<IFileStorage>(fileStorage);
        _container.RegisterInstanceAs(pathProvider);
        _container.RegisterInstanceAs<ITaskRepository>(repository);
        _container.RegisterInstanceAs<ITaskService>(taskService);

        // Register test drivers
        _container.RegisterTypeAs<TodoCliDriver, TodoCliDriver>();
        _container.RegisterTypeAs<TaskStorageDriver, TaskStorageDriver>();
    }

    [AfterScenario]
    public void AfterScenario()
    {
        // Clean up test data
        var storageDriver = _container.Resolve<TaskStorageDriver>();
        storageDriver.CleanupTestData();
    }
}
