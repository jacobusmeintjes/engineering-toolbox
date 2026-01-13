using TodoCli.Models;

namespace TodoCli.Services;

/// <summary>
/// Business logic service for task operations
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Creates and adds a new task
    /// </summary>
    Task<TodoTask> AddTaskAsync(string title, string? description = null, DateOnly? dueDate = null,
        Priority priority = Priority.Medium, List<string>? tags = null);

    /// <summary>
    /// Retrieves all tasks (optionally filtered and sorted)
    /// </summary>
    Task<List<TodoTask>> GetTasksAsync(TaskFilter? filter = null);

    /// <summary>
    /// Retrieves a single task by ID or partial ID
    /// </summary>
    Task<TodoTask?> GetTaskByIdAsync(string idOrPartial);

    /// <summary>
    /// Marks a task as complete
    /// </summary>
    Task<TodoTask> CompleteTaskAsync(string idOrPartial);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TodoTask> UpdateTaskAsync(string idOrPartial, string? newTitle = null, string? newDescription = null,
        DateOnly? newDueDate = null, Priority? newPriority = null, List<string>? addTags = null, List<string>? removeTags = null,
        bool clearDueDate = false);

    /// <summary>
    /// Deletes a task permanently
    /// </summary>
    Task DeleteTaskAsync(string idOrPartial);
}
