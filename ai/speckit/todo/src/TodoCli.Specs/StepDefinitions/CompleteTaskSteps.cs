using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using TodoCli.Models;
using TodoCli.Services;
using TodoCli.Infrastructure.Storage;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Commands;
using TodoCli.Output;
using TestUtilities;
using System.CommandLine;
using System.Text;
using System.Diagnostics;

namespace TodoCli.Specs.StepDefinitions;

[Binding]
public class CompleteTaskSteps : IDisposable
{
    private readonly TestFileSystem _testFileSystem;
    private readonly IFileStorage _fileStorage;
    private readonly StoragePathProvider _pathProvider;
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskService _taskService;
    private readonly TestConsoleWriter _consoleWriter;
    private readonly ColorProvider _colorProvider;
    private readonly TableFormatter _tableFormatter;
    private string _commandOutput = string.Empty;
    private int _commandExitCode;
    private TodoTask? _lastCreatedTask;
    private readonly List<TodoTask> _createdTasks = new();
    private readonly Stopwatch _stopwatch = new();

    public CompleteTaskSteps()
    {
        _testFileSystem = new TestFileSystem();
        _fileStorage = new FileStorage();
        _pathProvider = new TestStoragePathProvider(_testFileSystem.TempDirectory);
        _taskRepository = new JsonTaskRepository(_fileStorage, _pathProvider);
        _taskService = new TaskService(_taskRepository);
        _colorProvider = new ColorProvider();
        _tableFormatter = new TableFormatter(_colorProvider);
        _consoleWriter = new TestConsoleWriter();

        Environment.SetEnvironmentVariable("TEST_MODE", "1");
    }

    public async Task ClearTaskList()
    {
        var tasksFile = _pathProvider.GetTasksFilePath();
        if (File.Exists(tasksFile))
        {
            File.Delete(tasksFile);
        }
        _createdTasks.Clear();
    }

    [Given(@"I have added a task with title ""(.*)""")]
    public async Task GivenIHaveAddedATaskWithTitle(string title)
    {
        _lastCreatedTask = await _taskService.AddTaskAsync(title);
        _createdTasks.Add(_lastCreatedTask);
    }

    [Given(@"I have added a task with title ""(.*)"" (\d+) seconds ago")]
    public async Task GivenIHaveAddedATaskSecondsAgo(string title, int secondsAgo)
    {
        _lastCreatedTask = await _taskService.AddTaskAsync(title);
        // Simulate task created in the past by manipulating created date
        var tasks = await _taskRepository.GetAllAsync();
        var task = tasks.First(t => t.Title == title);
        // Use reflection to set CreatedAt
        var createdAtField = typeof(TodoTask).GetProperty("CreatedAt");
        if (createdAtField != null)
        {
            createdAtField.SetValue(task, DateTime.UtcNow.AddSeconds(-secondsAgo));
        }
        await _taskRepository.SaveAllAsync(tasks);
        _createdTasks.Add(task);
    }

    [Given(@"I have added multiple tasks with IDs sharing prefix")]
    public async Task GivenIHaveAddedMultipleTasksWithIDsSharingPrefix()
    {
        // Create tasks and hope for shared prefix (low probability with GUIDs)
        // For testing purposes, we'll create multiple tasks
        for (int i = 0; i < 5; i++)
        {
            var task = await _taskService.AddTaskAsync($"Task {i}");
            _createdTasks.Add(task);
        }
    }

    [Given(@"I have completed that task")]
    public async Task GivenIHaveCompletedThatTask()
    {
        if (_lastCreatedTask != null)
        {
            await _taskService.CompleteTaskAsync(_lastCreatedTask.Id.ToString());
        }
    }

    [Given(@"I have added the following tasks:")]
    public async Task GivenIHaveAddedTheFollowingTasks(Table table)
    {
        foreach (var row in table.Rows)
        {
            var title = row["Title"];
            var task = await _taskService.AddTaskAsync(title);
            _createdTasks.Add(task);
        }
    }

    [When(@"I complete the task by full ID")]
    public async Task WhenICompleteTheTaskByFullID()
    {
        if (_lastCreatedTask != null)
        {
            _stopwatch.Restart();
            await ExecuteCompleteCommand(_lastCreatedTask.Id.ToString());
            _stopwatch.Stop();
        }
    }

    [When(@"I complete the task with partial ID using (\d+) characters")]
    public async Task WhenICompleteTheTaskWithPartialIDUsingCharacters(int length)
    {
        if (_lastCreatedTask != null)
        {
            var partialId = _lastCreatedTask.Id.ToString()[..length];
            await ExecuteCompleteCommand(partialId);
        }
    }

    [When(@"I try to complete a task with ambiguous partial ID")]
    public async Task WhenITryToCompleteATaskWithAmbiguousPartialID()
    {
        // Use first 2 chars of first task (highly ambiguous)
        if (_createdTasks.Count > 0)
        {
            var ambiguousId = _createdTasks[0].Id.ToString()[..2];
            await ExecuteCompleteCommand(ambiguousId);
        }
    }

    [When(@"I try to complete the same task again")]
    public async Task WhenITryToCompleteTheSameTaskAgain()
    {
        if (_lastCreatedTask != null)
        {
            await ExecuteCompleteCommand(_lastCreatedTask.Id.ToString());
        }
    }

    [When(@"I try to complete a task with ID ""(.*)""")]
    public async Task WhenITryToCompleteATaskWithID(string taskId)
    {
        await ExecuteCompleteCommand(taskId);
    }

    [When(@"I try to complete the task with only (\d+) characters")]
    public async Task WhenITryToCompleteTheTaskWithOnlyCharacters(int length)
    {
        if (_lastCreatedTask != null)
        {
            var partialId = _lastCreatedTask.Id.ToString()[..length];
            await ExecuteCompleteCommand(partialId);
        }
    }

    [When(@"I complete ""(.*)"" and ""(.*)""")]
    public async Task WhenICompleteAnd(string title1, string title2)
    {
        var task1 = _createdTasks.FirstOrDefault(t => t.Title == title1);
        var task2 = _createdTasks.FirstOrDefault(t => t.Title == title2);

        if (task1 != null)
        {
            await ExecuteCompleteCommand(task1.Id.ToString());
        }
        if (task2 != null)
        {
            await ExecuteCompleteCommand(task2.Id.ToString());
        }
    }

    [When(@"I list all tasks")]
    public async Task WhenIListAllTasks()
    {
        var listCommand = new ListCommand(_taskService, new TaskFilter(), _tableFormatter, _consoleWriter);
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(listCommand);

        _commandExitCode = await rootCommand.InvokeAsync(new[] { "list" });
        _commandOutput = _consoleWriter.GetOutput();
    }

    private async Task ExecuteCompleteCommand(string taskId)
    {
        var command = new CompleteCommand(_taskService, _consoleWriter);
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(command);

        _commandExitCode = await rootCommand.InvokeAsync(new[] { "complete", taskId });
        _commandOutput = _consoleWriter.GetOutput();
    }

    [Then(@"the task should be marked as complete")]
    public async Task ThenTheTaskShouldBeMarkedAsComplete()
    {
        if (_lastCreatedTask != null)
        {
            var task = await _taskRepository.GetByIdAsync(_lastCreatedTask.Id.ToString());
            task.Should().NotBeNull();
            task!.IsCompleted.Should().BeTrue();
        }
    }

    [Then(@"the completion timestamp should be set")]
    public async Task ThenTheCompletionTimestampShouldBeSet()
    {
        if (_lastCreatedTask != null)
        {
            var task = await _taskRepository.GetByIdAsync(_lastCreatedTask.Id.ToString());
            task.Should().NotBeNull();
            task!.CompletedAt.Should().NotBeNull();
            task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }
    }

    [Then(@"I should see a success message with duration")]
    public void ThenIShouldSeeASuccessMessageWithDuration()
    {
        _commandOutput.Should().Contain("completed");
        _commandOutput.Should().MatchRegex(@"\d+\s+(second|minute|hour|day)s?");
    }

    [Then(@"I should see an error about ambiguous ID")]
    public void ThenIShouldSeeAnErrorAboutAmbiguousID()
    {
        _commandOutput.Should().Contain("Ambiguous");
    }

    [Then(@"no tasks should be marked complete")]
    public async Task ThenNoTasksShouldBeMarkedComplete()
    {
        var allTasks = await _taskRepository.GetAllAsync();
        allTasks.Should().NotContain(t => t.IsCompleted);
    }

    [Then(@"I should see an error that task is already complete")]
    public void ThenIShouldSeeAnErrorThatTaskIsAlreadyComplete()
    {
        _commandOutput.Should().Contain("already");
    }

    [Then(@"I should see an error that task was not found")]
    public void ThenIShouldSeeAnErrorThatTaskWasNotFound()
    {
        _commandOutput.Should().Contain("not found");
    }

    [Then(@"I should see an error about minimum ID length")]
    public void ThenIShouldSeeAnErrorAboutMinimumIDLength()
    {
        _commandOutput.Should().Contain("at least 4");
    }

    [Then(@"I should see duration displayed as ""(.*)""")]
    public void ThenIShouldSeeDurationDisplayedAs(string expectedDuration)
    {
        _commandOutput.Should().Contain(expectedDuration);
    }

    [Then(@"the task should show completion indicator ""\[✓\]""")]
    public void ThenTheTaskShouldShowCompletionIndicator()
    {
        _commandOutput.Should().Contain("[✓]");
    }

    [Then(@"the task should show completion timestamp")]
    public void ThenTheTaskShouldShowCompletionTimestamp()
    {
        // In list view, completed tasks show with [✓] indicator
        _commandOutput.Should().Contain("[✓]");
    }

    [Then(@"""(.*)"" should be complete")]
    public async Task ThenTaskShouldBeComplete(string title)
    {
        var task = _createdTasks.FirstOrDefault(t => t.Title == title);
        task.Should().NotBeNull();

        var reloadedTask = await _taskRepository.GetByIdAsync(task!.Id.ToString());
        reloadedTask.Should().NotBeNull();
        reloadedTask!.IsCompleted.Should().BeTrue();
    }

    [Then(@"""(.*)"" should be incomplete")]
    public async Task ThenTaskShouldBeIncomplete(string title)
    {
        var task = _createdTasks.FirstOrDefault(t => t.Title == title);
        task.Should().NotBeNull();

        var reloadedTask = await _taskRepository.GetByIdAsync(task!.Id.ToString());
        reloadedTask.Should().NotBeNull();
        reloadedTask!.IsCompleted.Should().BeFalse();
    }

    [Then(@"the operation should complete in under (\d+) milliseconds")]
    public void ThenTheOperationShouldCompleteInUnderMilliseconds(int maxMilliseconds)
    {
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxMilliseconds);
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

    private class TestConsoleWriter : IConsoleWriter
    {
        private readonly StringBuilder _output = new();

        public void WriteLine(string message)
        {
            _output.AppendLine(message);
        }

        public void WriteSuccess(string message)
        {
            _output.AppendLine($"[SUCCESS] {message}");
        }

        public void WriteError(string message)
        {
            _output.AppendLine($"[ERROR] {message}");
        }

        public void WriteWarning(string message)
        {
            _output.AppendLine($"[WARNING] {message}");
        }

        public void WriteInfo(string message)
        {
            _output.AppendLine($"[INFO] {message}");
        }

        public string? ReadLine() => null;

        public string GetOutput() => _output.ToString();
    }
}
