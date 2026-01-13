using System.CommandLine;
using TodoCli.Models;
using TodoCli.Services;
using TodoCli.Output;

namespace TodoCli.Commands;

public class ListCommand : Command
{
    private readonly ITaskService _taskService;
    private readonly TaskFilter _taskFilter;
    private readonly TableFormatter _tableFormatter;
    private readonly IConsoleWriter _consoleWriter;

    public ListCommand(
        ITaskService taskService,
        TaskFilter taskFilter,
        TableFormatter tableFormatter,
        IConsoleWriter consoleWriter) : base("list", "List tasks with filtering and sorting")
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _taskFilter = taskFilter ?? throw new ArgumentNullException(nameof(taskFilter));
        _tableFormatter = tableFormatter ?? throw new ArgumentNullException(nameof(tableFormatter));
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));

        // Filter options
        var statusOption = new Option<string?>(
            new[] { "--status", "-s" },
            "Filter by status (all, complete, incomplete)");

        var priorityOption = new Option<Priority?>(
            new[] { "--priority", "-p" },
            "Filter by priority (High, Medium, Low)");

        var tagsOption = new Option<string?>(
            new[] { "--tags", "-t" },
            "Filter by tags (comma-separated, OR logic)");

        var dueOption = new Option<string?>(
            new[] { "--due", "-d" },
            "Filter by due date (overdue, today, week, month)");

        // Sort option
        var sortOption = new Option<string>(
            new[] { "--sort" },
            () => "created",
            "Sort by (created, due, priority)");

        AddOption(statusOption);
        AddOption(priorityOption);
        AddOption(tagsOption);
        AddOption(dueOption);
        AddOption(sortOption);

        this.SetHandler(ExecuteAsync, statusOption, priorityOption, tagsOption, dueOption, sortOption);
    }

    private async Task<int> ExecuteAsync(
        string? status,
        Priority? priority,
        string? tags,
        string? due,
        string sort)
    {
        try
        {
            var allTasks = await _taskService.GetTasksAsync();

            // Apply filters
            var filteredTasks = _taskFilter.Apply(allTasks, status, priority, tags, due);

            // Apply sorting
            filteredTasks = ApplySort(filteredTasks, sort);

            if (!filteredTasks.Any())
            {
                _consoleWriter.WriteInfo("No tasks found");
                return 0;
            }

            // Format and display
            // Use RenderTaskTable for rich console output with colors
            var taskList = filteredTasks.ToList();
            if (Console.IsOutputRedirected || Environment.GetEnvironmentVariable("TEST_MODE") == "1")
            {
                // In test mode or redirected output, use plain text table
                var table = _tableFormatter.FormatTaskTable(taskList);
                _consoleWriter.WriteLine(table);
            }
            else
            {
                // In normal mode, use rich Spectre.Console output
                _tableFormatter.RenderTaskTable(taskList);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _consoleWriter.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private IEnumerable<TodoTask> ApplySort(IEnumerable<TodoTask> tasks, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "created" => tasks.OrderByDescending(t => t.CreatedAt),
            "due" => tasks.OrderBy(t => t.DueDate ?? DateOnly.MaxValue),
            "priority" => tasks.OrderByDescending(t => t.Priority),
            _ => tasks.OrderByDescending(t => t.CreatedAt)
        };
    }
}
