using System.CommandLine;
using TodoCli.Services;
using TodoCli.Output;

namespace TodoCli.Commands;

public class DeleteCommand : Command
{
    private readonly ITaskService _taskService;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ColorProvider _colorProvider;

    public DeleteCommand(
        ITaskService taskService,
        IConsoleWriter consoleWriter,
        ColorProvider colorProvider) : base("delete", "Delete a task permanently")
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
        _colorProvider = colorProvider ?? throw new ArgumentNullException(nameof(colorProvider));

        var idArgument = new Argument<string>(
            "id",
            "Task ID or partial ID (minimum 4 characters)");

        var forceOption = new Option<bool>(
            new[] { "--force", "-f" },
            "Skip confirmation prompt");

        AddArgument(idArgument);
        AddOption(forceOption);

        this.SetHandler(ExecuteAsync, idArgument, forceOption);
    }

    private async Task<int> ExecuteAsync(string idOrPartial, bool force)
    {
        try
        {
            if (idOrPartial.Length < 4)
            {
                _consoleWriter.WriteError("Task ID must be at least 4 characters");
                return 1;
            }

            // Get the task to display details before deletion
            var task = await _taskService.GetTaskByIdAsync(idOrPartial);
            if (task == null)
            {
                _consoleWriter.WriteError($"Task not found: {idOrPartial}");
                return 1;
            }

            // Show task details and confirmation prompt (unless --force)
            if (!force)
            {
                _consoleWriter.WriteLine("");
                _consoleWriter.WriteLine(_colorProvider.Yellow("⚠ Warning: This action cannot be undone!"));
                _consoleWriter.WriteLine("");
                _consoleWriter.WriteLine($"Task to delete:");
                _consoleWriter.WriteLine($"  ID: {task.GetShortId()}");
                _consoleWriter.WriteLine($"  Title: {task.Title}");
                if (!string.IsNullOrEmpty(task.Description))
                {
                    _consoleWriter.WriteLine($"  Description: {task.Description}");
                }
                _consoleWriter.WriteLine($"  Priority: {task.Priority}");
                if (task.Tags.Count > 0)
                {
                    _consoleWriter.WriteLine($"  Tags: {string.Join(", ", task.Tags)}");
                }
                _consoleWriter.WriteLine("");

                Console.Write("Are you sure you want to delete this task? (yes/no): ");
                var response = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (response != "yes" && response != "y")
                {
                    _consoleWriter.WriteInfo("Delete cancelled");
                    return 0;
                }
            }

            // Perform deletion
            await _taskService.DeleteTaskAsync(idOrPartial);

            _consoleWriter.WriteSuccess($"Task deleted: {task.Title} [{task.GetShortId()}]");

            return 0;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Ambiguous"))
        {
            _consoleWriter.WriteError($"Ambiguous ID '{idOrPartial}'. Please provide more characters.");
            return 1;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
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
}
