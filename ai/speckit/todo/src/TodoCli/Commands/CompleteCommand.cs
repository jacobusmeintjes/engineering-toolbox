using System.CommandLine;
using TodoCli.Services;
using TodoCli.Output;

namespace TodoCli.Commands;

public class CompleteCommand : Command
{
    private readonly ITaskService _taskService;
    private readonly IConsoleWriter _consoleWriter;

    public CompleteCommand(
        ITaskService taskService,
        IConsoleWriter consoleWriter) : base("complete", "Mark a task as complete")
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));

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
            // Validate minimum ID length
            if (idOrPartial.Length < 4)
            {
                _consoleWriter.WriteError("Task ID must be at least 4 characters");
                return 1;
            }

            // Complete the task
            var task = await _taskService.CompleteTaskAsync(idOrPartial);

            // Calculate duration
            var duration = task.GetCompletionDuration();
            var durationText = duration.HasValue ? FormatDuration(duration.Value) : "unknown duration";

            // Display success message
            _consoleWriter.WriteSuccess($"Task completed: {task.Title} (took {durationText})");

            return 0;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Ambiguous"))
        {
            _consoleWriter.WriteError($"Ambiguous ID '{idOrPartial}'. Please provide more characters.");
            return 1;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
        {
            _consoleWriter.WriteError($"Task is already complete");
            return 1;
        }
        catch (KeyNotFoundException)
        {
            _consoleWriter.WriteError($"Task not found: {idOrPartial}");
            return 1;
        }
        catch (Exception ex)
        {
            _consoleWriter.WriteError($"Error: {ex.Message}");
            return 1;
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
