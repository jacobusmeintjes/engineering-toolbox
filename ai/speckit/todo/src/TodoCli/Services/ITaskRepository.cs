using TodoCli.Models;

namespace TodoCli.Services;

/// <summary>
/// Abstraction for task persistence operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Retrieves all tasks from storage
    /// </summary>
    Task<List<TodoTask>> GetAllAsync();

    /// <summary>
    /// Saves all tasks to storage
    /// </summary>
    Task SaveAllAsync(List<TodoTask> tasks);

    /// <summary>
    /// Finds a task by its full or partial ID
    /// </summary>
    Task<TodoTask?> GetByIdAsync(string idOrPartial);

    /// <summary>
    /// Finds all tasks matching a partial ID (for disambiguation)
    /// </summary>
    Task<List<TodoTask>> FindByPartialIdAsync(string partialId);
}
