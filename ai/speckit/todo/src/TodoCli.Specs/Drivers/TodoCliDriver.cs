using TodoCli.Services;

namespace TodoCli.Specs.Drivers;

/// <summary>
/// Test automation driver for CLI command execution
/// </summary>
public class TodoCliDriver
{
    private readonly ITaskService _taskService;

    public TodoCliDriver(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public ITaskService TaskService => _taskService;

    // Helper methods for common test operations will be added as needed
}
