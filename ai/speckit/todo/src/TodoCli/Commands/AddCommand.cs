using System.CommandLine;
using TodoCli.Models;
using TodoCli.Output;
using TodoCli.Services;

namespace TodoCli.Commands;

/// <summary>
/// Implements the 'add' command for creating new tasks
/// </summary>
public class AddCommand : Command
{
    public AddCommand() : base("add", "Add a new task")
    {
        // Required argument: title
        var titleArgument = new Argument<string>(
            name: "title",
            description: "Task title (1-200 characters)"
        );

        // Optional options
        var descriptionOption = new Option<string?>(
            aliases: new[] { "--description", "-d" },
            description: "Extended task description (0-1000 characters)"
        );

        var dueDateOption = new Option<DateOnly?>(
            aliases: new[] { "--due" },
            description: "Due date in YYYY-MM-DD format (must be today or future)"
        );

        var priorityOption = new Option<Priority>(
            aliases: new[] { "--priority", "-p" },
            getDefaultValue: () => Priority.Medium,
            description: "Task priority: low, medium, or high"
        );

        var tagsOption = new Option<string?>(
            aliases: new[] { "--tags", "-t" },
            description: "Comma-separated tags (max 10, each 1-20 chars, alphanumeric + hyphens/underscores)"
        );

        AddArgument(titleArgument);
        AddOption(descriptionOption);
        AddOption(dueDateOption);
        AddOption(priorityOption);
        AddOption(tagsOption);

        this.SetHandler(ExecuteAsync, titleArgument, descriptionOption, dueDateOption, priorityOption, tagsOption);
    }

    private static async Task<int> ExecuteAsync(
        string title,
        string? description,
        DateOnly? dueDate,
        Priority priority,
        string? tagsString)
    {
        // Get services from DI (will be set up in Program.cs)
        var serviceProvider = ServiceProviderAccessor.ServiceProvider;
        if (serviceProvider == null)
        {
            Console.WriteLine("✗ Error: Service provider not initialized");
            return 1;
        }

        var taskService = serviceProvider.GetService(typeof(ITaskService)) as ITaskService;
        var consoleWriter = serviceProvider.GetService(typeof(IConsoleWriter)) as IConsoleWriter;

        if (taskService == null || consoleWriter == null)
        {
            Console.WriteLine("✗ Error: Required services not available");
            return 1;
        }

        try
        {
            // Parse tags
            List<string>? tags = null;
            if (!string.IsNullOrWhiteSpace(tagsString))
            {
                tags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
            }

            // Create task
            var task = await taskService.AddTaskAsync(title, description, dueDate, priority, tags);

            // Display success message
            consoleWriter.WriteSuccess($"Task added: {task.Title} [ID: {task.GetShortId()}]");

            return 0; // Success
        }
        catch (ArgumentException ex)
        {
            consoleWriter.WriteError(ex.Message);
            return 1; // Validation error
        }
        catch (Exception ex)
        {
            consoleWriter.WriteError($"Failed to add task: {ex.Message}");
            return 1; // General error
        }
    }
}

/// <summary>
/// Static accessor for service provider (set in Program.cs)
/// </summary>
public static class ServiceProviderAccessor
{
    public static IServiceProvider? ServiceProvider { get; set; }
}
