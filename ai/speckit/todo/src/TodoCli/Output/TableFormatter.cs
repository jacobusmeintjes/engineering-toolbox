using Spectre.Console;
using TodoCli.Models;

namespace TodoCli.Output;

/// <summary>
/// Formats tasks as rich terminal tables using Spectre.Console
/// </summary>
public class TableFormatter
{
    private readonly ColorProvider _colorProvider;

    public TableFormatter(ColorProvider colorProvider)
    {
        _colorProvider = colorProvider ?? throw new ArgumentNullException(nameof(colorProvider));
    }

    /// <summary>
    /// Renders a list of tasks as a formatted table
    /// </summary>
    public void RenderTaskTable(List<TodoTask> tasks)
    {
        if (tasks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No tasks found.[/]");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);

        // Add columns (total ~80 characters for standard terminal)
        table.AddColumn(new TableColumn("[bold]ID[/]").Width(8));
        table.AddColumn(new TableColumn("[bold]Status[/]").Width(3));
        table.AddColumn(new TableColumn("[bold]Title[/]").Width(30));
        table.AddColumn(new TableColumn("[bold]Pri[/]").Width(3));
        table.AddColumn(new TableColumn("[bold]Due[/]").Width(12));
        table.AddColumn(new TableColumn("[bold]Tags[/]").Width(15));

        foreach (var task in tasks)
        {
            table.AddRow(
                FormatId(task),
                FormatStatus(task),
                FormatTitle(task),
                FormatPriority(task),
                FormatDueDate(task),
                FormatTags(task)
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]Total: {tasks.Count} task(s)[/]");
    }

    /// <summary>
    /// Formats tasks as a string table (for testing and non-Spectre output)
    /// </summary>
    public string FormatTaskTable(IEnumerable<TodoTask> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
        {
            return "No tasks found";
        }

        var lines = new List<string>();

        // Header (total 80 chars)
        lines.Add("┌────────┬─────┬─────────────────────────┬─────┬──────────┬────────────┐");
        lines.Add("│ ID     │ Sta │ Title                   │ Pri │ Due      │ Tags       │");
        lines.Add("├────────┼─────┼─────────────────────────┼─────┼──────────┼────────────┤");

        // Rows
        foreach (var task in taskList)
        {
            var id = task.GetShortId().PadRight(6);
            var status = FormatStatusPlain(task).PadRight(3);
            var title = FormatTitlePlain(task).PadRight(23);
            var priority = FormatPriorityPlain(task).PadRight(3);
            var dueDate = FormatDueDatePlain(task).PadRight(8);
            var tags = FormatTagsPlain(task).PadRight(10);

            lines.Add($"│ {id} │ {status} │ {title} │ {priority} │ {dueDate} │ {tags} │");
        }

        // Footer
        lines.Add("└────────┴─────┴─────────────────────────┴─────┴──────────┴────────────┘");

        return string.Join("\n", lines);
    }

    private string FormatStatusPlain(TodoTask task)
    {
        return task.IsCompleted ? "[✓]" : "[ ]";
    }

    private string FormatTitlePlain(TodoTask task)
    {
        return task.Title.Length > 23 ? task.Title[..20] + "..." : task.Title;
    }

    private string FormatPriorityPlain(TodoTask task)
    {
        return task.Priority switch
        {
            Priority.High => "[H]",
            Priority.Medium => "[M]",
            Priority.Low => "[L]",
            _ => "[-]"
        };
    }

    private string FormatDueDatePlain(TodoTask task)
    {
        if (!task.DueDate.HasValue)
            return "-";

        var dueDate = task.DueDate.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (dueDate < today)
        {
            return $"{dueDate:MM/dd}[!]"; // e.g., "01/10[!]"
        }
        else if (dueDate == today)
        {
            return $"{dueDate:MM/dd}";
        }
        else
        {
            return $"{dueDate:MM/dd}";
        }
    }

    private string FormatTagsPlain(TodoTask task)
    {
        if (task.Tags.Count == 0)
            return "-";

        var tagsText = string.Join(", ", task.Tags);
        return tagsText.Length > 10 ? tagsText[..7] + "..." : tagsText;
    }

    private string FormatId(TodoTask task)
    {
        return $"[dim]{task.GetShortId()}[/]";
    }

    private string FormatStatus(TodoTask task)
    {
        return task.IsCompleted
            ? "[green]✓[/]"
            : "[dim] [/]";
    }

    private string FormatTitle(TodoTask task)
    {
        var title = task.Title.Length > 28
            ? task.Title[..28] + "..."
            : task.Title;

        if (task.IsCompleted)
        {
            return $"[green strike]{title.EscapeMarkup()}[/]";
        }

        return title.EscapeMarkup();
    }

    private string FormatPriority(TodoTask task)
    {
        return task.Priority switch
        {
            Priority.High => "[red]H[/]",
            Priority.Medium => "[yellow]M[/]",
            Priority.Low => "[grey]L[/]",
            _ => "[dim]-[/]"
        };
    }

    private string FormatDueDate(TodoTask task)
    {
        if (!task.DueDate.HasValue)
            return "[dim]-[/]";

        var dueDate = task.DueDate.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (dueDate < today)
        {
            return $"[red]{dueDate:yyyy-MM-dd} ![/]";
        }
        else if (dueDate == today)
        {
            return $"[yellow]{dueDate:yyyy-MM-dd}[/]";
        }
        else
        {
            return $"[dim]{dueDate:yyyy-MM-dd}[/]";
        }
    }

    private string FormatTags(TodoTask task)
    {
        if (task.Tags.Count == 0)
            return "[dim]-[/]";

        var tagsText = string.Join(", ", task.Tags);
        if (tagsText.Length > 13)
        {
            tagsText = tagsText[..13] + "...";
        }

        return $"[cyan]{tagsText.EscapeMarkup()}[/]";
    }
}
