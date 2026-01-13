using System.Text.Json.Serialization;

namespace TodoCli.Models;

/// <summary>
/// Represents a single TODO task with title, optional metadata, and completion tracking.
/// </summary>
public class TodoTask
{
    /// <summary>
    /// Unique identifier for the task (auto-generated, immutable)
    /// </summary>
    public Guid Id { get; init; }

    private string _title = string.Empty;
    /// <summary>
    /// Main description of the task (required, 1-200 characters)
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Task title cannot be empty");

            var trimmed = value.Trim();
            if (trimmed.Length > 200)
                throw new ArgumentException("Task title cannot exceed 200 characters");

            _title = trimmed;
        }
    }

    private string? _description;
    /// <summary>
    /// Extended details about the task (optional, 0-1000 characters)
    /// </summary>
    public string? Description
    {
        get => _description;
        set
        {
            if (value == null)
            {
                _description = null;
                return;
            }

            var trimmed = value.Trim();
            if (trimmed.Length > 1000)
                throw new ArgumentException("Task description cannot exceed 1000 characters");

            _description = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }

    /// <summary>
    /// When the task was created (auto-set UTC, immutable)
    /// </summary>
    public DateTime CreatedAt { get; init; }

    private DateOnly? _dueDate;
    /// <summary>
    /// Target completion date (optional, must be today or future)
    /// </summary>
    public DateOnly? DueDate
    {
        get => _dueDate;
        set
        {
            if (value.HasValue && value.Value < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Due date must be today or in the future");

            _dueDate = value;
        }
    }

    /// <summary>
    /// Sets the due date without validation (for testing purposes only)
    /// </summary>
    internal void SetDueDateForTesting(DateOnly? dueDate)
    {
        _dueDate = dueDate;
    }

    private bool _isCompleted;
    /// <summary>
    /// Completion status (one-way: false → true only)
    /// </summary>
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted && !value)
                throw new InvalidOperationException("Cannot uncomplete a task (one-way transition only)");

            _isCompleted = value;

            // Auto-set CompletedAt when marking complete
            if (value && CompletedAt == null)
            {
                CompletedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// When the task was completed (auto-set when IsCompleted=true, immutable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Importance level (Low, Medium (default), or High)
    /// </summary>
    public Priority Priority { get; set; } = Priority.Medium;

    private List<string> _tags = new();
    /// <summary>
    /// Categorization labels (0-10 tags, each 1-20 chars, lowercase, alphanumeric + hyphens/underscores)
    /// </summary>
    public List<string> Tags
    {
        get => _tags;
        set
        {
            ValidateTags(value);
            _tags = value;
        }
    }

    /// <summary>
    /// Creates a new TodoTask with auto-generated ID and UTC timestamp
    /// </summary>
    public TodoTask()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        _tags = new List<string>();
    }

    /// <summary>
    /// Creates a TodoTask with specified values (for deserialization)
    /// </summary>
    [JsonConstructor]
    public TodoTask(Guid id, string title, string? description, DateTime createdAt, DateOnly? dueDate,
        bool isCompleted, DateTime? completedAt, Priority priority, List<string> tags)
    {
        Id = id;
        CreatedAt = createdAt;
        _title = title; // Bypass validation for deserialization
        _description = description;
        _dueDate = dueDate;
        _isCompleted = isCompleted;
        CompletedAt = completedAt;
        Priority = priority;
        _tags = tags ?? new List<string>();

        // Validate after construction
        if (string.IsNullOrWhiteSpace(_title) || _title.Length > 200)
            throw new ArgumentException("Invalid task title in deserialized data");

        if (_description?.Length > 1000)
            throw new ArgumentException("Invalid task description in deserialized data");

        ValidateTags(_tags);
        ValidateCompletionConsistency();
    }

    /// <summary>
    /// Marks the task as complete with timestamp
    /// </summary>
    public void Complete()
    {
        if (_isCompleted)
            throw new InvalidOperationException("Task is already completed");

        IsCompleted = true;
    }

    /// <summary>
    /// Adds tags to the task (case-insensitive, prevents duplicates)
    /// </summary>
    public void AddTags(params string[] newTags)
    {
        foreach (var tag in newTags)
        {
            var normalized = NormalizeTag(tag);
            if (!_tags.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                _tags.Add(normalized);
            }
        }

        if (_tags.Count > 10)
            throw new ArgumentException("Maximum 10 tags allowed per task");
    }

    /// <summary>
    /// Removes tags from the task (case-insensitive, silently ignores non-existent tags)
    /// </summary>
    public void RemoveTags(params string[] tagsToRemove)
    {
        foreach (var tag in tagsToRemove)
        {
            _tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Calculates duration from creation to completion
    /// </summary>
    public TimeSpan? GetCompletionDuration()
    {
        if (!IsCompleted || CompletedAt == null)
            return null;

        return CompletedAt.Value - CreatedAt;
    }

    /// <summary>
    /// Gets the shortened ID (first 8 characters) for display
    /// </summary>
    public string GetShortId() => Id.ToString()[..8];

    private static void ValidateTags(List<string> tags)
    {
        if (tags.Count > 10)
            throw new ArgumentException("Maximum 10 tags allowed per task");

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag cannot be empty");

            if (tag.Length > 20)
                throw new ArgumentException($"Tag '{tag}' exceeds 20 character limit");

            if (!System.Text.RegularExpressions.Regex.IsMatch(tag, @"^[a-zA-Z0-9_-]+$"))
                throw new ArgumentException($"Tag '{tag}' contains invalid characters (use only letters, numbers, hyphens, underscores)");
        }
    }

    private static string NormalizeTag(string tag)
    {
        var trimmed = tag.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Tag cannot be empty");

        if (trimmed.Length > 20)
            throw new ArgumentException($"Tag '{trimmed}' exceeds 20 character limit");

        var normalized = trimmed.ToLowerInvariant();

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^[a-z0-9_-]+$"))
            throw new ArgumentException($"Tag '{trimmed}' contains invalid characters (use only letters, numbers, hyphens, underscores)");

        return normalized;
    }

    private void ValidateCompletionConsistency()
    {
        if (_isCompleted && CompletedAt == null)
            throw new InvalidOperationException("Completed task must have CompletedAt timestamp");

        if (!_isCompleted && CompletedAt != null)
            throw new InvalidOperationException("Incomplete task cannot have CompletedAt timestamp");

        if (CompletedAt.HasValue && CompletedAt.Value < CreatedAt)
            throw new InvalidOperationException("CompletedAt cannot be before CreatedAt");
    }
}
