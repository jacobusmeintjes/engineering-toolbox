using TodoCli.Models;

namespace TodoCli.Services;

/// <summary>
/// Encapsulates filtering and sorting logic for task queries
/// </summary>
public class TaskFilter
{
    public enum CompletionStatus
    {
        All,
        Complete,
        Incomplete
    }

    public enum SortBy
    {
        Created,
        Due,
        Priority
    }

    public CompletionStatus Status { get; set; } = CompletionStatus.Incomplete;
    public Priority? PriorityFilter { get; set; }
    public List<string>? Tags { get; set; }
    public DateOnly? DueBefore { get; set; }
    public SortBy SortOrder { get; set; } = SortBy.Created;

    /// <summary>
    /// Applies filtering and sorting to a list of tasks
    /// </summary>
    public List<TodoTask> Apply(List<TodoTask> tasks)
    {
        var query = tasks.AsEnumerable();

        // Filter by completion status
        query = Status switch
        {
            CompletionStatus.Complete => query.Where(t => t.IsCompleted),
            CompletionStatus.Incomplete => query.Where(t => !t.IsCompleted),
            _ => query // All
        };

        // Filter by priority
        if (PriorityFilter.HasValue)
        {
            query = query.Where(t => t.Priority == PriorityFilter.Value);
        }

        // Filter by tags (OR logic: match if task has ANY of the specified tags)
        if (Tags != null && Tags.Count > 0)
        {
            query = query.Where(t => Tags.Any(tag => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        // Filter by due date
        if (DueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= DueBefore.Value);
        }

        // Sort
        query = SortOrder switch
        {
            SortBy.Priority => query.OrderByDescending(t => (int)t.Priority).ThenBy(t => t.CreatedAt),
            SortBy.Due => query.OrderBy(t => t.DueDate ?? DateOnly.MaxValue).ThenBy(t => t.CreatedAt),
            _ => query.OrderBy(t => t.CreatedAt) // Default: Created
        };

        return query.ToList();
    }

    /// <summary>
    /// Applies filtering with string-based parameters (for command line usage)
    /// </summary>
    public IEnumerable<TodoTask> Apply(
        IEnumerable<TodoTask> tasks,
        string? status = null,
        Priority? priority = null,
        string? tags = null,
        string? due = null)
    {
        var query = tasks;

        // Filter by completion status
        if (!string.IsNullOrEmpty(status))
        {
            query = status.ToLowerInvariant() switch
            {
                "complete" => query.Where(t => t.IsCompleted),
                "incomplete" => query.Where(t => !t.IsCompleted),
                "all" => query,
                _ => query
            };
        }

        // Filter by priority
        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        // Filter by tags (OR logic)
        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => t.Trim())
                             .ToList();
            query = query.Where(t => tagList.Any(tag =>
                t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        // Filter by due date
        if (!string.IsNullOrEmpty(due))
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            query = due.ToLowerInvariant() switch
            {
                "overdue" => query.Where(t => t.DueDate.HasValue && t.DueDate.Value < today),
                "today" => query.Where(t => t.DueDate.HasValue && t.DueDate.Value == today),
                "week" => query.Where(t => t.DueDate.HasValue &&
                    t.DueDate.Value >= today &&
                    t.DueDate.Value <= today.AddDays(7)),
                "month" => query.Where(t => t.DueDate.HasValue &&
                    t.DueDate.Value >= today &&
                    t.DueDate.Value <= today.AddDays(30)),
                _ => query
            };
        }

        return query;
    }
}
