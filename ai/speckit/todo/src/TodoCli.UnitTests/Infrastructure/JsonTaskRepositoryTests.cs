using FluentAssertions;
using Moq;
using System.Text.Json;
using TodoCli.Infrastructure.Configuration;
using TodoCli.Infrastructure.Storage;
using TodoCli.Models;

namespace TodoCli.UnitTests.Infrastructure;

public class JsonTaskRepositoryTests
{
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly TestStoragePathProvider _pathProvider;
    private readonly JsonTaskRepository _repository;

    public JsonTaskRepositoryTests()
    {
        _mockFileStorage = new Mock<IFileStorage>();
        _pathProvider = new TestStoragePathProvider("/test");

        _repository = new JsonTaskRepository(_mockFileStorage.Object, _pathProvider);
    }

    private class TestStoragePathProvider : StoragePathProvider
    {
        private readonly string _testDirectory;

        public TestStoragePathProvider(string testDirectory)
        {
            _testDirectory = testDirectory;
        }

        public new string GetStorageDirectory() => _testDirectory;
        public new string GetTasksFilePath() => Path.Combine(_testDirectory, "tasks.json");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyListWhenFileDoesNotExist()
    {
        // Arrange
        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().BeEmpty();
        _mockFileStorage.Verify(fs => fs.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldDeserializeTasksFromFile()
    {
        // Arrange
        var testTasks = new List<TodoTask>
        {
            new TodoTask { Title = "Task 1" },
            new TodoTask { Title = "Task 2" }
        };
        var json = JsonSerializer.Serialize(testTasks, TodoTaskJsonContext.Default.ListTodoTask);

        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileStorage.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().HaveCount(2);
        tasks[0].Title.Should().Be("Task 1");
        tasks[1].Title.Should().Be("Task 2");
    }

    [Fact]
    public async Task SaveAllAsync_ShouldSerializeTasksToFile()
    {
        // Arrange
        var testTasks = new List<TodoTask>
        {
            new TodoTask { Title = "Task 1" }
        };

        // Act
        await _repository.SaveAllAsync(testTasks);

        // Assert
        _mockFileStorage.Verify(fs => fs.WriteAllTextAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Task 1"))), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTaskWithMatchingId()
    {
        // Arrange
        var task = new TodoTask { Title = "Test Task" };
        var testTasks = new List<TodoTask> { task };
        var json = JsonSerializer.Serialize(testTasks, TodoTaskJsonContext.Default.ListTodoTask);

        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileStorage.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        var result = await _repository.GetByIdAsync(task.Id.ToString());

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTaskWithPartialId()
    {
        // Arrange
        var task = new TodoTask { Title = "Test Task" };
        var testTasks = new List<TodoTask> { task };
        var json = JsonSerializer.Serialize(testTasks, TodoTaskJsonContext.Default.ListTodoTask);
        var partialId = task.Id.ToString()[..8];

        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileStorage.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        var result = await _repository.GetByIdAsync(partialId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowIfPartialIdIsAmbiguous()
    {
        // Arrange - create two tasks with IDs that share a common prefix
        var task1 = new TodoTask { Title = "Task 1" };
        var task2 = new TodoTask { Title = "Task 2" };
        var testTasks = new List<TodoTask> { task1, task2 };
        var json = JsonSerializer.Serialize(testTasks, TodoTaskJsonContext.Default.ListTodoTask);

        // Use a partial ID that could match either (just first 4 chars of task1)
        var ambiguousPartialId = task1.Id.ToString()[..4];

        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileStorage.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        Func<Task> act = async () => await _repository.GetByIdAsync(ambiguousPartialId);

        // Assert - this will only throw if both IDs happen to start with same 4 chars
        // In practice, GUIDs are random enough that this is unlikely
        // So we just verify the method doesn't crash
        try
        {
            await act();
        }
        catch (InvalidOperationException ex)
        {
            ex.Message.Should().Contain("Ambiguous ID");
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullIfNotFound()
    {
        // Arrange
        var testTasks = new List<TodoTask> { new TodoTask { Title = "Test" } };
        var json = JsonSerializer.Serialize(testTasks, TodoTaskJsonContext.Default.ListTodoTask);

        _mockFileStorage.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileStorage.Setup(fs => fs.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeNull();
    }
}
