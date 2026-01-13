using TodoCli.Models;

namespace TodoCli.Services;

/// <summary>
/// Implements business logic for task operations
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<TodoTask> AddTaskAsync(string title, string? description = null, DateOnly? dueDate = null,
        Priority priority = Priority.Medium, List<string>? tags = null)
    {
        var task = new TodoTask
        {
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority
        };

        if (tags != null && tags.Count > 0)
        {
            task.AddTags(tags.ToArray());
        }

        var allTasks = await _repository.GetAllAsync();
        allTasks.Add(task);
        await _repository.SaveAllAsync(allTasks);

        return task;
    }

    public async Task<List<TodoTask>> GetTasksAsync(TaskFilter? filter = null)
    {
        var tasks = await _repository.GetAllAsync();

        if (filter == null)
        {
            // Default: show incomplete tasks, sorted by created date
            filter = new TaskFilter
            {
                Status = TaskFilter.CompletionStatus.Incomplete,
                SortOrder = TaskFilter.SortBy.Created
            };
        }

        return filter.Apply(tasks);
    }

    public async Task<TodoTask?> GetTaskByIdAsync(string idOrPartial)
    {
        return await _repository.GetByIdAsync(idOrPartial);
    }

    public async Task<TodoTask> CompleteTaskAsync(string idOrPartial)
    {
        var allTasks = await _repository.GetAllAsync();
        var task = allTasks.FirstOrDefault(t =>
            t.Id.ToString().StartsWith(idOrPartial, StringComparison.OrdinalIgnoreCase));

        if (task == null)
        {
            // Try exact match
            if (Guid.TryParse(idOrPartial, out var exactId))
            {
                task = allTasks.FirstOrDefault(t => t.Id == exactId);
            }
        }

        if (task == null)
            throw new KeyNotFoundException($"Task not found with ID: {idOrPartial}");

        // Check for ambiguous IDs
        if (idOrPartial.Length < 36) // Not a full GUID
        {
            var matches = allTasks.Where(t =>
                t.Id.ToString().StartsWith(idOrPartial, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Ambiguous ID '{idOrPartial}' matches {matches.Count} tasks");
            }
        }

        task.Complete();
        await _repository.SaveAllAsync(allTasks);

        return task;
    }

    public async Task<TodoTask> UpdateTaskAsync(string idOrPartial, string? newTitle = null, string? newDescription = null,
        DateOnly? newDueDate = null, Priority? newPriority = null, List<string>? addTags = null, List<string>? removeTags = null,
        bool clearDueDate = false)
    {
        var allTasks = await _repository.GetAllAsync();
        var task = allTasks.FirstOrDefault(t =>
            t.Id.ToString().StartsWith(idOrPartial, StringComparison.OrdinalIgnoreCase));

        if (task == null)
        {
            // Try exact match
            if (Guid.TryParse(idOrPartial, out var exactId))
            {
                task = allTasks.FirstOrDefault(t => t.Id == exactId);
            }
        }

        if (task == null)
            throw new InvalidOperationException($"Task not found with ID: {idOrPartial}");

        // Check for ambiguous IDs
        if (idOrPartial.Length < 36) // Not a full GUID
        {
            var matches = allTasks.Where(t =>
                t.Id.ToString().StartsWith(idOrPartial, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Ambiguous ID '{idOrPartial}' matches {matches.Count} tasks");
            }
        }

        // Update fields (partial update - only specified fields change)
        if (newTitle != null)
            task.Title = newTitle;

        if (newDescription != null)
            task.Description = newDescription;

        if (clearDueDate)
            task.DueDate = null;
        else if (newDueDate.HasValue)
            task.DueDate = newDueDate.Value;

        if (newPriority.HasValue)
            task.Priority = newPriority.Value;

        if (addTags != null && addTags.Count > 0)
            task.AddTags(addTags.ToArray());

        if (removeTags != null && removeTags.Count > 0)
            task.RemoveTags(removeTags.ToArray());

        await _repository.SaveAllAsync(allTasks);

        return task;
    }

    public async Task DeleteTaskAsync(string idOrPartial)
    {
        var task = await _repository.GetByIdAsync(idOrPartial);

        if (task == null)
            throw new InvalidOperationException($"Task not found with ID: {idOrPartial}");

        var allTasks = await _repository.GetAllAsync();
        allTasks.RemoveAll(t => t.Id == task.Id);
        await _repository.SaveAllAsync(allTasks);
    }
}
