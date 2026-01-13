using FluentAssertions;
using TestUtilities;
using TodoCli.Infrastructure.Storage;

namespace TodoCli.UnitTests.Infrastructure;

public class FileStorageTests : IDisposable
{
    private readonly TestFileSystem _testFileSystem;
    private readonly FileStorage _fileStorage;

    public FileStorageTests()
    {
        _testFileSystem = new TestFileSystem();
        _fileStorage = new FileStorage();
    }

    [Fact]
    public async Task WriteAllTextAsync_ShouldCreateFileWithContent()
    {
        // Arrange
        var filePath = Path.Combine(_testFileSystem.TempDirectory, "test.txt");
        var content = "Test content";

        // Act
        await _fileStorage.WriteAllTextAsync(filePath, content);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Be(content);
    }

    [Fact]
    public async Task WriteAllTextAsync_ShouldCreateBackupBeforeWrite()
    {
        // Arrange
        var filePath = Path.Combine(_testFileSystem.TempDirectory, "test.txt");
        var backupPath = $"{filePath}.bak";

        // Write initial content
        await File.WriteAllTextAsync(filePath, "Original content");

        // Act
        await _fileStorage.WriteAllTextAsync(filePath, "New content");

        // Assert
        File.Exists(backupPath).Should().BeTrue();
        var backupContent = await File.ReadAllTextAsync(backupPath);
        backupContent.Should().Be("Original content");
    }

    [Fact]
    public async Task WriteAllTextAsync_ShouldUseAtomicWritePattern()
    {
        // Arrange
        var filePath = Path.Combine(_testFileSystem.TempDirectory, "test.txt");
        var tempPath = $"{filePath}.tmp";

        // Act
        await _fileStorage.WriteAllTextAsync(filePath, "Test");

        // Assert
        File.Exists(filePath).Should().BeTrue();
        File.Exists(tempPath).Should().BeFalse(); // Temp file should be cleaned up
    }

    [Fact]
    public async Task ReadAllTextAsync_ShouldReturnFileContent()
    {
        // Arrange
        var filePath = _testFileSystem.CreateTempFile("test.txt", "Test content");

        // Act
        var content = await _fileStorage.ReadAllTextAsync(filePath);

        // Assert
        content.Should().Be("Test content");
    }

    [Fact]
    public async Task ReadAllTextAsync_ShouldThrowIfFileNotFound()
    {
        // Arrange
        var filePath = Path.Combine(_testFileSystem.TempDirectory, "nonexistent.txt");

        // Act
        Func<Task> act = async () => await _fileStorage.ReadAllTextAsync(filePath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public void FileExists_ShouldReturnTrueForExistingFile()
    {
        // Arrange
        var filePath = _testFileSystem.CreateTempFile("test.txt");

        // Act
        var exists = _fileStorage.FileExists(filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void FileExists_ShouldReturnFalseForNonExistentFile()
    {
        // Arrange
        var filePath = Path.Combine(_testFileSystem.TempDirectory, "nonexistent.txt");

        // Act
        var exists = _fileStorage.FileExists(filePath);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void EnsureDirectoryExists_ShouldCreateDirectory()
    {
        // Arrange
        var dirPath = Path.Combine(_testFileSystem.TempDirectory, "subdir");

        // Act
        _fileStorage.EnsureDirectoryExists(dirPath);

        // Assert
        Directory.Exists(dirPath).Should().BeTrue();
    }

    [Fact]
    public void CreateBackup_ShouldCopyFileToBackupPath()
    {
        // Arrange
        var filePath = _testFileSystem.CreateTempFile("test.txt", "Original");
        var backupPath = $"{filePath}.bak";

        // Act
        _fileStorage.CreateBackup(filePath);

        // Assert
        File.Exists(backupPath).Should().BeTrue();
        File.ReadAllText(backupPath).Should().Be("Original");
    }

    [Fact]
    public void RestoreFromBackup_ShouldCopyBackupToMainFile()
    {
        // Arrange
        var filePath = _testFileSystem.CreateTempFile("test.txt", "Current");
        var backupPath = $"{filePath}.bak";
        File.WriteAllText(backupPath, "Backup");

        // Act
        _fileStorage.RestoreFromBackup(filePath);

        // Assert
        File.ReadAllText(filePath).Should().Be("Backup");
    }

    public void Dispose()
    {
        _testFileSystem.Dispose();
    }
}
