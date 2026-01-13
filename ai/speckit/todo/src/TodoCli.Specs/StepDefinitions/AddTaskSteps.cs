using FluentAssertions;
using System.Diagnostics;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TodoCli.Models;
using TodoCli.Services;
using TodoCli.Specs.Drivers;

namespace TodoCli.Specs.StepDefinitions;

[Binding]
public class AddTaskSteps
{
    private readonly ITaskService _taskService;
    private readonly TaskStorageDriver _storageDriver;
    private TodoTask? _lastCreatedTask;
    private Exception? _lastException;
    private TimeSpan _lastOperationDuration;

    public AddTaskSteps(ITaskService taskService, TaskStorageDriver storageDriver)
    {
        _taskService = taskService;
        _storageDriver = storageDriver;
    }

    [Given(@"the task storage is empty")]
    public void GivenTheTaskStorageIsEmpty()
    {
        _storageDriver.CleanupTestData();
    }

    [When(@"I add a task with title ""(.*)""")]
    public async Task WhenIAddATaskWithTitle(string title)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync(title);
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
        stopwatch.Stop();
        _lastOperationDuration = stopwatch.Elapsed;
    }

    [When(@"I add a task with the following details:")]
    public async Task WhenIAddATaskWithTheFollowingDetails(Table table)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var details = table.Rows[0];
            var title = details["Title"];
            var description = details.ContainsKey("Description") ? details["Description"] : null;
            var dueDate = details.ContainsKey("Due") ? DateOnly.Parse(details["Due"]) : (DateOnly?)null;
            var priority = details.ContainsKey("Priority") ? Enum.Parse<Priority>(details["Priority"]) : Priority.Medium;
            var tags = details.ContainsKey("Tags") ? details["Tags"].Split(',').ToList() : null;

            _lastCreatedTask = await _taskService.AddTaskAsync(title, description, dueDate, priority, tags);
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
        stopwatch.Stop();
        _lastOperationDuration = stopwatch.Elapsed;
    }

    [When(@"I attempt to add a task with empty title")]
    public async Task WhenIAttemptToAddATaskWithEmptyTitle()
    {
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync("");
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
    }

    [When(@"I attempt to add a task with title longer than (.*) characters")]
    public async Task WhenIAttemptToAddATaskWithTitleLongerThanCharacters(int maxLength)
    {
        var longTitle = new string('A', maxLength + 1);
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync(longTitle);
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
    }

    [When(@"I attempt to add a task with due date in the past")]
    public async Task WhenIAttemptToAddATaskWithDueDateInThePast()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync("Test task", dueDate: pastDate);
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
    }

    [When(@"I attempt to add a task with tag ""(.*)""")]
    public async Task WhenIAttemptToAddATaskWithTag(string tag)
    {
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync("Test task", tags: new List<string> { tag });
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
    }

    [When(@"I attempt to add a task with (.*) tags")]
    public async Task WhenIAttemptToAddATaskWithTags(int tagCount)
    {
        var tags = Enumerable.Range(1, tagCount).Select(i => $"tag{i}").ToList();
        try
        {
            _lastCreatedTask = await _taskService.AddTaskAsync("Test task", tags: tags);
        }
        catch (Exception ex)
        {
            _lastException = ex;
        }
    }

    [Then(@"the task should be saved with ID")]
    public void ThenTheTaskShouldBeSavedWithID()
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.Id.Should().NotBeEmpty();
    }

    [Then(@"the task title should be ""(.*)""")]
    public void ThenTheTaskTitleShouldBe(string expectedTitle)
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.Title.Should().Be(expectedTitle);
    }

    [Then(@"the task description should be ""(.*)""")]
    public void ThenTheTaskDescriptionShouldBe(string expectedDescription)
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.Description.Should().Be(expectedDescription);
    }

    [Then(@"the task due date should be ""(.*)""")]
    public void ThenTheTaskDueDateShouldBe(string expectedDueDate)
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.DueDate.Should().Be(DateOnly.Parse(expectedDueDate));
    }

    [Then(@"the task priority should be ""(.*)""")]
    public void ThenTheTaskPriorityShouldBe(string expectedPriority)
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.Priority.Should().Be(Enum.Parse<Priority>(expectedPriority));
    }

    [Then(@"the task tags should contain ""(.*)""")]
    public void ThenTheTaskTagsShouldContain(string expectedTag)
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.Tags.Should().Contain(expectedTag.ToLowerInvariant());
    }

    [Then(@"the task should be incomplete")]
    public void ThenTheTaskShouldBeIncomplete()
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.IsCompleted.Should().BeFalse();
    }

    [Then(@"the task should have creation timestamp")]
    public void ThenTheTaskShouldHaveCreationTimestamp()
    {
        _lastCreatedTask.Should().NotBeNull();
        _lastCreatedTask!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Then(@"there should be (.*) tasks in storage")]
    public async Task ThenThereShouldBeTasksInStorage(int expectedCount)
    {
        var tasks = await _taskService.GetTasksAsync(new TaskFilter { Status = TaskFilter.CompletionStatus.All });
        tasks.Count.Should().Be(expectedCount);
    }

    [Then(@"I should receive an error ""(.*)""")]
    public void ThenIShouldReceiveAnError(string expectedErrorMessage)
    {
        _lastException.Should().NotBeNull();
        _lastException!.Message.Should().Contain(expectedErrorMessage);
    }

    [Then(@"the operation should complete in under (.*) milliseconds")]
    public void ThenTheOperationShouldCompleteInUnderMilliseconds(int maxMilliseconds)
    {
        _lastOperationDuration.TotalMilliseconds.Should().BeLessThan(maxMilliseconds);
    }
}
