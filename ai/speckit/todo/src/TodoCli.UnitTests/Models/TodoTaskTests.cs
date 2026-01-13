using FluentAssertions;
using TodoCli.Models;

namespace TodoCli.UnitTests.Models;

public class TodoTaskTests
{
    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Arrange & Act
        var task1 = new TodoTask { Title = "Task 1" };
        var task2 = new TodoTask { Title = "Task 2" };

        // Assert
        task1.Id.Should().NotBe(Guid.Empty);
        task2.Id.Should().NotBe(Guid.Empty);
        task1.Id.Should().NotBe(task2.Id);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange & Act
        var task = new TodoTask { Title = "Test Task" };

        // Assert
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Title_ShouldRejectEmptyString()
    {
        // Arrange
        var task = new TodoTask();

        // Act
        Action act = () => task.Title = "";

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task title cannot be empty");
    }

    [Fact]
    public void Title_ShouldRejectWhitespaceOnly()
    {
        // Arrange
        var task = new TodoTask();

        // Act
        Action act = () => task.Title = "   ";

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task title cannot be empty");
    }

    [Fact]
    public void Title_ShouldRejectTitleOver200Characters()
    {
        // Arrange
        var task = new TodoTask();
        var longTitle = new string('A', 201);

        // Act
        Action act = () => task.Title = longTitle;

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task title cannot exceed 200 characters");
    }

    [Fact]
    public void Title_ShouldTrimWhitespace()
    {
        // Arrange
        var task = new TodoTask();

        // Act
        task.Title = "  Test Task  ";

        // Assert
        task.Title.Should().Be("Test Task");
    }

    [Fact]
    public void Description_ShouldAcceptNull()
    {
        // Arrange & Act
        var task = new TodoTask { Title = "Test", Description = null };

        // Assert
        task.Description.Should().BeNull();
    }

    [Fact]
    public void Description_ShouldRejectOver1000Characters()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        var longDescription = new string('A', 1001);

        // Act
        Action act = () => task.Description = longDescription;

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task description cannot exceed 1000 characters");
    }

    [Fact]
    public void DueDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        // Act
        task.DueDate = futureDate;

        // Assert
        task.DueDate.Should().Be(futureDate);
    }

    [Fact]
    public void DueDate_ShouldAcceptToday()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        task.DueDate = today;

        // Assert
        task.DueDate.Should().Be(today);
    }

    [Fact]
    public void DueDate_ShouldRejectPastDate()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        // Act
        Action act = () => task.DueDate = pastDate;

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Due date must be today or in the future");
    }

    [Fact]
    public void IsCompleted_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var task = new TodoTask { Title = "Test" };

        // Assert
        task.IsCompleted.Should().BeFalse();
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Complete_ShouldSetIsCompletedAndTimestamp()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        task.Complete();

        // Assert
        task.IsCompleted.Should().BeTrue();
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_ShouldThrowIfAlreadyCompleted()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        task.Complete();

        // Act
        Action act = () => task.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Task is already completed");
    }

    [Fact]
    public void IsCompleted_ShouldPreventUncompleting()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        task.Complete();

        // Act
        Action act = () => task.IsCompleted = false;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot uncomplete a task*");
    }

    [Fact]
    public void Priority_ShouldDefaultToMedium()
    {
        // Arrange & Act
        var task = new TodoTask { Title = "Test" };

        // Assert
        task.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void AddTags_ShouldNormalizeToLowercase()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        task.AddTags("WORK", "Urgent");

        // Assert
        task.Tags.Should().Contain("work");
        task.Tags.Should().Contain("urgent");
    }

    [Fact]
    public void AddTags_ShouldPreventDuplicates()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        task.AddTags("work", "WORK", "Work");

        // Assert
        task.Tags.Should().HaveCount(1);
        task.Tags.Should().Contain("work");
    }

    [Fact]
    public void AddTags_ShouldRejectInvalidCharacters()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        Action act = () => task.AddTags("work@home");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*contains invalid characters*");
    }

    [Fact]
    public void AddTags_ShouldRejectOver10Tags()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        Action act = () => task.AddTags("tag1", "tag2", "tag3", "tag4", "tag5",
            "tag6", "tag7", "tag8", "tag9", "tag10", "tag11");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum 10 tags allowed per task");
    }

    [Fact]
    public void RemoveTags_ShouldRemoveExistingTags()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        task.AddTags("work", "urgent");

        // Act
        task.RemoveTags("urgent");

        // Assert
        task.Tags.Should().Contain("work");
        task.Tags.Should().NotContain("urgent");
    }

    [Fact]
    public void RemoveTags_ShouldIgnoreNonExistentTags()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        task.AddTags("work");

        // Act
        Action act = () => task.RemoveTags("nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GetCompletionDuration_ShouldReturnNullForIncompleteTasks()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };

        // Act
        var duration = task.GetCompletionDuration();

        // Assert
        duration.Should().BeNull();
    }

    [Fact]
    public void GetCompletionDuration_ShouldCalculateDurationForCompletedTasks()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        Thread.Sleep(100); // Ensure some time passes
        task.Complete();

        // Act
        var duration = task.GetCompletionDuration();

        // Assert
        duration.Should().NotBeNull();
        duration.Value.TotalMilliseconds.Should().BeGreaterThan(50);
    }

    [Fact]
    public void GetShortId_ShouldReturnFirst8Characters()
    {
        // Arrange
        var task = new TodoTask { Title = "Test" };
        var fullId = task.Id.ToString();

        // Act
        var shortId = task.GetShortId();

        // Assert
        shortId.Should().HaveLength(8);
        shortId.Should().Be(fullId[..8]);
    }
}
