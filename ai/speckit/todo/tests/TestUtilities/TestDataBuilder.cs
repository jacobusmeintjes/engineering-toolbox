using TodoCli.Models;

namespace TestUtilities;

/// <summary>
/// Fluent builder for creating test TodoTask instances
/// </summary>
public class TestDataBuilder
{
    private string _title = "Test Task";
    private string? _description;
    private DateOnly? _dueDate;
    private Priority _priority = Priority.Medium;
    private List<string> _tags = new();
    private bool _isCompleted;

    public static TestDataBuilder CreateTask() => new();

    public TestDataBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TestDataBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TestDataBuilder WithDueDate(DateOnly dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public TestDataBuilder WithPriority(Priority priority)
    {
        _priority = priority;
        return this;
    }

    public TestDataBuilder WithTags(params string[] tags)
    {
        _tags = tags.ToList();
        return this;
    }

    public TestDataBuilder AsCompleted()
    {
        _isCompleted = true;
        return this;
    }

    public TodoTask Build()
    {
        var task = new TodoTask
        {
            Title = _title,
            Description = _description,
            DueDate = _dueDate,
            Priority = _priority
        };

        if (_tags.Count > 0)
        {
            task.AddTags(_tags.ToArray());
        }

        if (_isCompleted)
        {
            task.Complete();
        }

        return task;
    }
}
