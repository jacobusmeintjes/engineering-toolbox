using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

/// <summary>
/// Document service implementation with business logic and authorization.
/// Handles document lifecycle: upload, browse, download, share, delete.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DocumentService> _logger;
    private readonly INotificationService _notificationService;
    
    // File validation constants
    private const long MaxFileSizeBytes = 26214400; // 25 MB
    private static readonly string[] AllowedExtensions = 
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"
    };

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        ILogger<DocumentService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
        _notificationService = notificationService;
    }

    #region Upload and Storage (US1)

    public async Task<Document> UploadAsync(Stream fileStream, string fileName, string contentType,
        string title, string? description, string category, string? tags, int? projectId, int uploadedByUserId)
    {
        try
        {
            // Validate file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            // Validate file size
            if (fileStream.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size ({fileStream.Length / 1024 / 1024:F2} MB) exceeds maximum allowed size (25 MB)");
            }

            // Upload file to storage
            var filePath = await _fileStorage.UploadAsync(fileStream, fileName, contentType);

            // Create document record
            var document = new Document
            {
                Title = title,
                Description = description,
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileStream.Length,
                FileType = contentType,
                Category = category,
                Tags = tags,
                ProjectId = projectId,
                UploadDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                UploadedByUserId = uploadedByUserId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document uploaded: {DocumentId} - {Title}", document.DocumentId, document.Title);
            
            // Send notifications to project members if document is linked to a project
            if (projectId.HasValue)
            {
                await SendProjectDocumentNotifications(document, uploadedByUserId);
            }
            
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    #endregion

    #region Browse and Organize (US2)

    public async Task<List<Document>> GetDocumentsByCategoryAsync(string category, int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId && d.Category == category)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<List<Document>> GetDocumentsByProjectAsync(int projectId, int userId)
    {
        // Check if user has access to the project
        var hasAccess = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        if (!hasAccess)
        {
            // Check if user is the project manager
            hasAccess = await _context.Projects
                .AnyAsync(p => p.ProjectId == projectId && p.ProjectManagerId == userId);
        }

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You do not have access to documents in this project");
        }

        return await _context.Documents
            .Where(d => d.ProjectId == projectId)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<List<Document>> SearchDocumentsAsync(string searchTerm, int userId)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        // Search in user's own documents
        var ownDocuments = _context.Documents
            .Where(d => d.UploadedByUserId == userId &&
                (d.Title.ToLower().Contains(lowerSearchTerm) ||
                 (d.Description != null && d.Description.ToLower().Contains(lowerSearchTerm)) ||
                 (d.Tags != null && d.Tags.ToLower().Contains(lowerSearchTerm))));

        // Search in shared documents
        var sharedDocuments = _context.DocumentShares
            .Where(ds => ds.SharedWithUserId == userId)
            .Select(ds => ds.Document)
            .Where(d => d.Title.ToLower().Contains(lowerSearchTerm) ||
                 (d.Description != null && d.Description.ToLower().Contains(lowerSearchTerm)) ||
                 (d.Tags != null && d.Tags.ToLower().Contains(lowerSearchTerm)));

        // Combine and remove duplicates
        var results = await ownDocuments.Union(sharedDocuments)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        return results;
    }

    public async Task<List<Document>> GetDocumentsSortedAsync(int userId, string sortBy, bool descending = false)
    {
        var query = _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .AsQueryable();

        query = sortBy.ToLower() switch
        {
            "title" => descending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "uploaddate" => descending ? query.OrderByDescending(d => d.UploadDate) : query.OrderBy(d => d.UploadDate),
            "category" => descending ? query.OrderByDescending(d => d.Category) : query.OrderBy(d => d.Category),
            "filesize" => descending ? query.OrderByDescending(d => d.FileSize) : query.OrderBy(d => d.FileSize),
            _ => query.OrderByDescending(d => d.UploadDate)
        };

        return await query.ToListAsync();
    }

    public async Task<List<Document>> GetDocumentsFilteredAsync(int userId, string? category = null,
        int? projectId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(d => d.Category == category);
        }

        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(d => d.UploadDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(d => d.UploadDate <= toDate.Value);
        }

        return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
    }

    #endregion

    #region Download and Management (US3)

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(int documentId, int userId)
    {
        var document = await GetDocumentByIdAsync(documentId, userId);
        
        var fileStream = await _fileStorage.DownloadAsync(document.FilePath);
        
        // Log download activity
        _logger.LogInformation("Document downloaded: {DocumentId} - {Title} by user {UserId}", 
            documentId, document.Title, userId);
        
        // Determine content type from document
        var contentType = document.FileType;
        
        return (fileStream, contentType, document.FileName);
    }

    public async Task<Document> GetDocumentByIdAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
                .ThenInclude(p => p.ProjectMembers)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document {documentId} not found");
        }

        // Check authorization: owner, has shared access, or is project member
        var isOwner = document.UploadedByUserId == userId;
        var hasSharedAccess = await _context.DocumentShares
            .AnyAsync(ds => ds.DocumentId == documentId && ds.SharedWithUserId == userId);
        var isProjectMember = document.ProjectId.HasValue && 
                             document.Project?.ProjectMembers?.Any(m => m.UserId == userId) == true;

        if (!isOwner && !hasSharedAccess && !isProjectMember)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this document");
        }

        return document;
    }

    public async Task UpdateDocumentAsync(int documentId, int userId, string? title = null,
        string? description = null, string? category = null, string? tags = null, int? projectId = null)
    {
        var document = await _context.Documents.FindAsync(documentId);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document {documentId} not found");
        }

        // Check authorization: only owner can update
        if (document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Only the document owner can update document metadata");
        }

        // Update fields if provided
        if (title != null) document.Title = title;
        if (description != null) document.Description = description;
        if (category != null) document.Category = category;
        if (tags != null) document.Tags = tags;
        if (projectId.HasValue) document.ProjectId = projectId.Value;
        
        document.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Document updated: {DocumentId}", documentId);
    }

    public async Task DeleteDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document {documentId} not found");
        }

        // Check authorization: only owner can delete
        if (document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Only the document owner can delete the document");
        }

        // Delete file from storage
        await _fileStorage.DeleteAsync(document.FilePath);

        // Remove database record (shares will be cascade deleted)
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Document deleted: {DocumentId}", documentId);
    }

    #endregion

    #region Sharing (US4)

    public async Task ShareDocumentAsync(int documentId, int sharedByUserId, int sharedWithUserId, bool canEdit = false)
    {
        var document = await _context.Documents.FindAsync(documentId);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document {documentId} not found");
        }

        // Check authorization: only owner can share
        if (document.UploadedByUserId != sharedByUserId)
        {
            throw new UnauthorizedAccessException("Only the document owner can share the document");
        }

        // Check if already shared with this user
        var existingShare = await _context.DocumentShares
            .FirstOrDefaultAsync(ds => ds.DocumentId == documentId && ds.SharedWithUserId == sharedWithUserId);

        if (existingShare != null)
        {
            throw new InvalidOperationException("Document is already shared with this user");
        }

        // Create share record
        var share = new DocumentShare
        {
            DocumentId = documentId,
            SharedByUserId = sharedByUserId,
            SharedWithUserId = sharedWithUserId,
            SharedDate = DateTime.UtcNow,
            CanEdit = canEdit
        };

        _context.DocumentShares.Add(share);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Document shared: {DocumentId} with user {UserId}", documentId, sharedWithUserId);
    }

    public async Task RevokeShareAsync(int shareId, int userId)
    {
        var share = await _context.DocumentShares
            .Include(ds => ds.Document)
            .FirstOrDefaultAsync(ds => ds.ShareId == shareId);

        if (share == null)
        {
            throw new KeyNotFoundException($"Share {shareId} not found");
        }

        // Check authorization: only document owner can revoke
        if (share.Document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Only the document owner can revoke shares");
        }

        _context.DocumentShares.Remove(share);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Share revoked: {ShareId}", shareId);
    }

    public async Task<List<Document>> GetSharedWithMeAsync(int userId)
    {
        return await _context.DocumentShares
            .Where(ds => ds.SharedWithUserId == userId)
            .Select(ds => ds.Document)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<List<DocumentShare>> GetDocumentSharesAsync(int documentId, int ownerId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document {documentId} not found");
        }

        // Check authorization: only owner can view shares
        if (document.UploadedByUserId != ownerId)
        {
            throw new UnauthorizedAccessException("Only the document owner can view shares");
        }

        return await _context.DocumentShares
            .Where(ds => ds.DocumentId == documentId)
            .Include(ds => ds.SharedWithUser)
            .Include(ds => ds.SharedByUser)
            .OrderByDescending(ds => ds.SharedDate)
            .ToListAsync();
    }

    public async Task UpdateSharePermissionsAsync(int shareId, int ownerId, bool canEdit)
    {
        var share = await _context.DocumentShares
            .Include(ds => ds.Document)
            .FirstOrDefaultAsync(ds => ds.ShareId == shareId);

        if (share == null)
        {
            throw new KeyNotFoundException($"Share {shareId} not found");
        }

        // Check authorization: only document owner can update permissions
        if (share.Document.UploadedByUserId != ownerId)
        {
            throw new UnauthorizedAccessException("Only the document owner can update share permissions");
        }

        share.CanEdit = canEdit;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Share permissions updated: {ShareId}, CanEdit={CanEdit}", shareId, canEdit);
    }

    #endregion

    #region Dashboard Integration (US5)

    public async Task<List<Document>> GetRecentDocumentsAsync(int userId, int count = 5)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .Include(d => d.UploadedBy)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetDocumentCountAsync(int userId)
    {
        return await _context.Documents
            .CountAsync(d => d.UploadedByUserId == userId);
    }

    public async Task<long> GetTotalStorageUsedAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .SumAsync(d => d.FileSize);
    }

    #endregion

    #region Task Integration (US5)

    public async Task AttachDocumentToTaskAsync(int documentId, int taskId, int userId)
    {
        // Verify document exists and user has access
        var document = await GetDocumentByIdAsync(documentId, userId);
        
        // Verify task exists
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }
        
        // Check if already attached
        var existingAttachment = await _context.TaskDocuments
            .AnyAsync(td => td.TaskId == taskId && td.DocumentId == documentId);
        
        if (existingAttachment)
        {
            throw new InvalidOperationException("Document is already attached to this task");
        }
        
        // Create attachment
        var taskDocument = new TaskDocument
        {
            TaskId = taskId,
            DocumentId = documentId,
            AttachedByUserId = userId,
            AttachedDate = DateTime.UtcNow
        };
        
        _context.TaskDocuments.Add(taskDocument);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Document {DocumentId} attached to task {TaskId} by user {UserId}", 
            documentId, taskId, userId);
    }

    public async Task<List<Document>> GetTaskDocumentsAsync(int taskId, int userId)
    {
        // Verify task exists
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }
        
        // Get documents attached to task
        return await _context.TaskDocuments
            .Where(td => td.TaskId == taskId)
            .Include(td => td.Document)
                .ThenInclude(d => d.UploadedBy)
            .Include(td => td.Document)
                .ThenInclude(d => d.Project)
            .Select(td => td.Document)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    #endregion
    
    #region Helper Methods
    
    private async Task SendProjectDocumentNotifications(Document document, int uploaderId)
    {
        if (!document.ProjectId.HasValue) return;
        
        try
        {
            // Get project members
            var projectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == document.ProjectId.Value && pm.UserId != uploaderId)
                .Select(pm => pm.UserId)
                .ToListAsync();
            
            // Send notification to each project member
            foreach (var userId in projectMembers)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "New Project Document",
                    Message = $"A new document '{document.Title}' has been added to your project",
                    Type = NotificationType.ProjectUpdate,
                    Priority = NotificationPriority.Informational,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                
                await _notificationService.CreateNotificationAsync(notification);
            }
            
            _logger.LogInformation("Sent {Count} notifications for document {DocumentId}", 
                projectMembers.Count, document.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending project document notifications for document {DocumentId}", 
                document.DocumentId);
            // Don't throw - notification failure shouldn't prevent document upload
        }
    }
    
    #endregion
}
