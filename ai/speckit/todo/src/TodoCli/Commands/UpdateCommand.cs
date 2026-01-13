using System.CommandLine;
using TodoCli.Services;
using TodoCli.Models;
using TodoCli.Output;

namespace TodoCli.Commands;

public class UpdateCommand : Command
{
    private readonly ITaskService _taskService;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ColorProvider _colorProvider;

    public UpdateCommand(
        ITaskService taskService,
        IConsoleWriter consoleWriter,
        ColorProvider colorProvider) : base("update", "Update task properties")
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
        _colorProvider = colorProvider ?? throw new ArgumentNullException(nameof(colorProvider));

        var idArgument = new Argument<string>(
            "id",
            "Task ID or partial ID (minimum 4 characters)");

        var titleOption = new Option<string?>(
            new[] { "--title", "-t" },
            "New task title");

        var descriptionOption = new Option<string?>(
            new[] { "--description", "-d" },
            "New task description");

        var priorityOption = new Option<Priority?>(
            new[] { "--priority", "-p" },
            "New priority (High, Medium, Low)");

        var dueDateOption = new Option<string?>(
            new[] { "--due" },
            "New due date (yyyy-mm-dd or 'none' to clear)");

        var addTagsOption = new Option<string?>(
            new[] { "--add-tags" },
            "Tags to add (comma-separated)");

        var removeTagsOption = new Option<string?>(
            new[] { "--remove-tags" },
            "Tags to remove (comma-separated)");

        AddArgument(idArgument);
        AddOption(titleOption);
        AddOption(descriptionOption);
        AddOption(priorityOption);
        AddOption(dueDateOption);
        AddOption(addTagsOption);
        AddOption(removeTagsOption);

        this.SetHandler(ExecuteAsync, idArgument, titleOption, descriptionOption,
            priorityOption, dueDateOption, addTagsOption, removeTagsOption);
    }

    private async Task<int> ExecuteAsync(
        string idOrPartial,
        string? newTitle,
        string? newDescription,
        Priority? newPriority,
        string? newDueDate,
        string? addTags,
        string? removeTags)
    {
        try
        {
            if (idOrPartial.Length < 4)
            {
                _consoleWriter.WriteError("Task ID must be at least 4 characters");
                return 1;
            }

            // Check if any update was specified
            if (newTitle == null && newDescription == null && newPriority == null &&
                newDueDate == null && addTags == null && removeTags == null)
            {
                _consoleWriter.WriteError("No changes specified. Use --title, --description, --priority, --due, --add-tags, or --remove-tags");
                return 1;
            }

            // Get the task before update to show before/after
            var originalTask = await _taskService.GetTaskByIdAsync(idOrPartial);
            if (originalTask == null)
            {
                _consoleWriter.WriteError($"Task not found: {idOrPartial}");
                return 1;
            }

            // Parse due date
            DateOnly? parsedDueDate = null;
            bool clearDueDate = false;

            if (newDueDate != null)
            {
                if (newDueDate.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    clearDueDate = true;
                }
                else if (DateOnly.TryParse(newDueDate, out var date))
                {
                    parsedDueDate = date;
                }
                else
                {
                    _consoleWriter.WriteError($"Invalid due date format: {newDueDate}. Use yyyy-mm-dd or 'none'");
                    return 1;
                }
            }

            // Parse tags
            List<string>? tagsToAdd = null;
            List<string>? tagsToRemove = null;

            if (addTags != null)
            {
                tagsToAdd = addTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
            }

            if (removeTags != null)
            {
                tagsToRemove = removeTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
            }

            // Perform the update
            var updatedTask = await _taskService.UpdateTaskAsync(
                idOrPartial,
                newTitle,
                newDescription,
                parsedDueDate,
                newPriority,
                tagsToAdd,
                tagsToRemove,
                clearDueDate);

            // Display before/after changes
            DisplayChanges(originalTask, updatedTask);

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
        catch (ArgumentException ex)
        {
            _consoleWriter.WriteError($"Invalid input: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            _consoleWriter.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private void DisplayChanges(TodoTask before, TodoTask after)
    {
        _consoleWriter.WriteLine("");
        _consoleWriter.WriteLine(_colorProvider.Green($"✓ Task updated: {after.GetShortId()}"));
        _consoleWriter.WriteLine("");

        // Title
        if (before.Title != after.Title)
        {
            _consoleWriter.WriteLine($"  Title:");
            _consoleWriter.WriteLine($"    {_colorProvider.Gray("Before:")} {before.Title}");
            _consoleWriter.WriteLine($"    {_colorProvider.Green("After:")}  {after.Title}");
        }

        // Description
        if (before.Description != after.Description)
        {
            _consoleWriter.WriteLine($"  Description:");
            _consoleWriter.WriteLine($"    {_colorProvider.Gray("Before:")} {before.Description ?? "(none)"}");
            _consoleWriter.WriteLine($"    {_colorProvider.Green("After:")}  {after.Description ?? "(none)"}");
        }

        // Priority
        if (before.Priority != after.Priority)
        {
            _consoleWriter.WriteLine($"  Priority:");
            _consoleWriter.WriteLine($"    {_colorProvider.Gray("Before:")} {before.Priority}");
            _consoleWriter.WriteLine($"    {_colorProvider.Green("After:")}  {after.Priority}");
        }

        // Due Date
        if (before.DueDate != after.DueDate)
        {
            _consoleWriter.WriteLine($"  Due Date:");
            _consoleWriter.WriteLine($"    {_colorProvider.Gray("Before:")} {(before.DueDate.HasValue ? before.DueDate.Value.ToString("yyyy-MM-dd") : "(none)")}");
            _consoleWriter.WriteLine($"    {_colorProvider.Green("After:")}  {(after.DueDate.HasValue ? after.DueDate.Value.ToString("yyyy-MM-dd") : "(none)")}");
        }

        // Tags
        var beforeTags = string.Join(", ", before.Tags);
        var afterTags = string.Join(", ", after.Tags);
        if (beforeTags != afterTags)
        {
            _consoleWriter.WriteLine($"  Tags:");
            _consoleWriter.WriteLine($"    {_colorProvider.Gray("Before:")} {(before.Tags.Count > 0 ? beforeTags : "(none)")}");
            _consoleWriter.WriteLine($"    {_colorProvider.Green("After:")}  {(after.Tags.Count > 0 ? afterTags : "(none)")}");
        }

        _consoleWriter.WriteLine("");
    }
}
