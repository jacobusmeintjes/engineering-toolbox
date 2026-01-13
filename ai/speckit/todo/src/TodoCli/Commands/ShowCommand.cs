using System.CommandLine;
using TodoCli.Services;
using TodoCli.Output;
using TodoCli.Models;

namespace TodoCli.Commands;

public class ShowCommand : Command
{
    private readonly ITaskService _taskService;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ColorProvider _colorProvider;

    public ShowCommand(
        ITaskService taskService,
        IConsoleWriter consoleWriter,
        ColorProvider colorProvider) : base("show", "Display detailed information about a task")
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
        _colorProvider = colorProvider ?? throw new ArgumentNullException(nameof(colorProvider));

        var idArgument = new Argument<string>(
            "id",
            "Task ID or partial ID (minimum 4 characters)");

        AddArgument(idArgument);

        this.SetHandler(ExecuteAsync, idArgument);
    }

    private async Task<int> ExecuteAsync(string idOrPartial)
    {
        try
        {
            if (idOrPartial.Length < 4)
            {
                _consoleWriter.WriteError("Task ID must be at least 4 characters");
                return 1;
            }

            var task = await _taskService.GetTaskByIdAsync(idOrPartial);

            if (task == null)
            {
                _consoleWriter.WriteError($"Task not found: {idOrPartial}");
                return 1;
            }

            DisplayTaskDetails(task);

            return 0;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Ambiguous"))
        {
            _consoleWriter.WriteError($"Ambiguous ID '{idOrPartial}'. Please provide more characters.");
            return 1;
        }
        catch (Exception ex)
        {
            _consoleWriter.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayTaskDetails(TodoTask task)
    {
        _consoleWriter.WriteLine("");
        _consoleWriter.WriteLine(_colorProvider.Cyan($"╔══════════════════════════════════════════════════════════════════╗"));
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Task Details: {task.GetShortId()}".PadRight(64) + _colorProvider.Cyan($"║"));
        _consoleWriter.WriteLine(_colorProvider.Cyan($"╠══════════════════════════════════════════════════════════════════╣"));

        // Title
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Title       : {task.Title}".PadRight(64) + _colorProvider.Cyan($"║"));

        // Status
        var status = task.IsCompleted
            ? _colorProvider.Green("Complete ✓")
            : _colorProvider.Yellow("Incomplete");
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Status      : {status}".PadRight(64) + _colorProvider.Cyan($"║"));

        // Priority
        var priorityColor = task.Priority switch
        {
            Priority.High => _colorProvider.Red($"High"),
            Priority.Medium => _colorProvider.Yellow($"Medium"),
            Priority.Low => _colorProvider.Gray($"Low"),
            _ => "Unknown"
        };
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Priority    : {priorityColor}".PadRight(64) + _colorProvider.Cyan($"║"));

        // Description
        var description = string.IsNullOrEmpty(task.Description) ? _colorProvider.Gray("(none)") : task.Description;
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Description : {description}".PadRight(64) + _colorProvider.Cyan($"║"));

        // Due Date
        var dueDate = task.DueDate.HasValue
            ? FormatDueDate(task.DueDate.Value)
            : _colorProvider.Gray("(not set)");
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Due Date    : {dueDate}".PadRight(64) + _colorProvider.Cyan($"║"));

        // Tags
        var tags = task.Tags.Count > 0
            ? string.Join(", ", task.Tags)
            : _colorProvider.Gray("(none)");
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Tags        : {tags}".PadRight(64) + _colorProvider.Cyan($"║"));

        _consoleWriter.WriteLine(_colorProvider.Cyan($"╠══════════════════════════════════════════════════════════════════╣"));

        // Created
        var age = DateTime.UtcNow - task.CreatedAt;
        _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Created     : {task.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC ({FormatAge(age)} ago)".PadRight(64) + _colorProvider.Cyan($"║"));

        // Completed (if applicable)
        if (task.IsCompleted && task.CompletedAt.HasValue)
        {
            var duration = task.GetCompletionDuration();
            var timeSinceCompletion = DateTime.UtcNow - task.CompletedAt.Value;
            _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Completed   : {task.CompletedAt:yyyy-MM-dd HH:mm:ss} UTC ({FormatAge(timeSinceCompletion)} ago)".PadRight(64) + _colorProvider.Cyan($"║"));
            if (duration.HasValue)
            {
                _consoleWriter.WriteLine(_colorProvider.Cyan($"║") + $" Duration    : {FormatDuration(duration.Value)}".PadRight(64) + _colorProvider.Cyan($"║"));
            }
        }

        _consoleWriter.WriteLine(_colorProvider.Cyan($"╚══════════════════════════════════════════════════════════════════╝"));
        _consoleWriter.WriteLine("");
    }

    private string FormatDueDate(DateOnly dueDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntil = dueDate.DayNumber - today.DayNumber;

        if (daysUntil < 0)
        {
            return _colorProvider.Red($"{dueDate:yyyy-MM-dd} (OVERDUE by {Math.Abs(daysUntil)} days!)");
        }
        else if (daysUntil == 0)
        {
            return _colorProvider.Yellow($"{dueDate:yyyy-MM-dd} (DUE TODAY!)");
        }
        else if (daysUntil <= 7)
        {
            return _colorProvider.Yellow($"{dueDate:yyyy-MM-dd} (in {daysUntil} days)");
        }
        else
        {
            return $"{dueDate:yyyy-MM-dd} (in {daysUntil} days)";
        }
    }

    private string FormatAge(TimeSpan age)
    {
        if (age.TotalDays >= 1)
        {
            var days = (int)age.TotalDays;
            return $"{days} day{(days != 1 ? "s" : "")}";
        }
        else if (age.TotalHours >= 1)
        {
            var hours = (int)age.TotalHours;
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        else if (age.TotalMinutes >= 1)
        {
            var minutes = (int)age.TotalMinutes;
            return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
        }
        else
        {
            var seconds = (int)age.TotalSeconds;
            return $"{seconds} second{(seconds != 1 ? "s" : "")}";
        }
    }

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
        {
            var days = (int)duration.TotalDays;
            return $"{days} day{(days != 1 ? "s" : "")}";
        }
        else if (duration.TotalHours >= 1)
        {
            var hours = (int)duration.TotalHours;
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        else if (duration.TotalMinutes >= 1)
        {
            var minutes = (int)duration.TotalMinutes;
            return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
        }
        else
        {
            var seconds = (int)duration.TotalSeconds;
            return $"{seconds} second{(seconds != 1 ? "s" : "")}";
        }
    }
}
