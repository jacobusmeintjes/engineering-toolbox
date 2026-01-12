namespace ContosoDashboard.Services;

/// <summary>
/// Abstraction for file storage operations supporting both local filesystem
/// and cloud storage implementations (e.g., Azure Blob Storage).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileStream">The file content stream</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <returns>Relative file path or blob name for storage reference</returns>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="filePath">Relative file path or blob name</param>
    /// <returns>File content stream</returns>
    Task<Stream> DownloadAsync(string filePath);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="filePath">Relative file path or blob name</param>
    Task DeleteAsync(string filePath);

    /// <summary>
    /// Gets a temporary URL for direct file access (for cloud implementations).
    /// </summary>
    /// <param name="filePath">Relative file path or blob name</param>
    /// <param name="expiration">URL expiration timespan</param>
    /// <returns>Temporary access URL (local implementation returns file path)</returns>
    Task<string> GetUrlAsync(string filePath, TimeSpan expiration);
}
