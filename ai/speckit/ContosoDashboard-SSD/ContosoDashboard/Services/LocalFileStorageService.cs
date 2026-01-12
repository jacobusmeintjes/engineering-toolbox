using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

/// <summary>
/// Local file system implementation of file storage service.
/// Uses System.IO.File operations for offline-first, training-focused storage.
/// Production systems should use cloud storage (Azure Blob Storage, AWS S3, etc.).
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        
        // Get configuration from appsettings.json
        _uploadPath = configuration["FileStorage:UploadPath"] 
            ?? throw new InvalidOperationException("FileStorage:UploadPath configuration is missing");
        
        _maxFileSizeBytes = configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 26214400); // Default 25 MB
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created upload directory: {UploadPath}", _uploadPath);
        }
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Validate file size
            if (fileStream.Length > _maxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size ({fileStream.Length} bytes) exceeds maximum allowed size ({_maxFileSizeBytes} bytes)");
            }

            // Generate unique file name to avoid collisions
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Save file to disk
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            _logger.LogInformation("Uploaded file: {FileName} -> {FilePath}", fileName, filePath);
            
            // Return relative path for storage in database
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Read file into memory stream to avoid file locking issues
            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            
            memoryStream.Position = 0;
            
            _logger.LogInformation("Downloaded file: {FilePath}", filePath);
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent file: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<string> GetUrlAsync(string filePath, TimeSpan expiration)
    {
        // For local file storage, return a relative URL that can be served by the app
        // In production with cloud storage, this would return a signed URL or CDN URL
        // Expiration parameter ignored for local storage
        var url = $"/api/documents/download/{filePath}";
        return Task.FromResult(url);
    }

    /// <summary>
    /// Determines MIME content type based on file extension.
    /// </summary>
    private string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }
}
