using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

/// <summary>
/// Service interface for document management business logic.
/// Provides methods for upload, download, search, sharing, and deletion operations.
/// </summary>
public interface IDocumentService
{
    // Upload and Storage (US1)
    Task<Document> UploadAsync(Stream fileStream, string fileName, string contentType, 
        string title, string? description, string category, string? tags, int? projectId, int uploadedByUserId);
    Task<List<Document>> GetUserDocumentsAsync(int userId);
    
    // Browse and Organize (US2)
    Task<List<Document>> GetDocumentsByCategoryAsync(string category, int userId);
    Task<List<Document>> GetDocumentsByProjectAsync(int projectId, int userId);
    Task<List<Document>> SearchDocumentsAsync(string searchTerm, int userId);
    Task<List<Document>> GetDocumentsSortedAsync(int userId, string sortBy, bool descending = false);
    Task<List<Document>> GetDocumentsFilteredAsync(int userId, string? category = null, 
        int? projectId = null, DateTime? fromDate = null, DateTime? toDate = null);
    
    // Download and Management (US3)
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(int documentId, int userId);
    Task<Document> GetDocumentByIdAsync(int documentId, int userId);
    Task UpdateDocumentAsync(int documentId, int userId, string? title = null, 
        string? description = null, string? category = null, string? tags = null, int? projectId = null);
    Task DeleteDocumentAsync(int documentId, int userId);
    
    // Sharing (US4)
    Task ShareDocumentAsync(int documentId, int sharedByUserId, int sharedWithUserId, bool canEdit = false);
    Task RevokeShareAsync(int shareId, int userId);
    Task<List<Document>> GetSharedWithMeAsync(int userId);
    Task<List<DocumentShare>> GetDocumentSharesAsync(int documentId, int ownerId);
    Task UpdateSharePermissionsAsync(int shareId, int ownerId, bool canEdit);
    
    // Dashboard Integration (US5)
    Task<List<Document>> GetRecentDocumentsAsync(int userId, int count = 5);
    Task<int> GetDocumentCountAsync(int userId);
    Task<long> GetTotalStorageUsedAsync(int userId);
    
    // Task Integration (US5)
    Task AttachDocumentToTaskAsync(int documentId, int taskId, int userId);
    Task<List<Document>> GetTaskDocumentsAsync(int taskId, int userId);
}
