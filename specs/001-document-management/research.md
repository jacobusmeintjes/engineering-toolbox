# Research: Document Upload and Management

**Feature**: Document Upload and Management  
**Branch**: 001-document-management  
**Date**: 2026-01-12

## Purpose

Resolve technical implementation questions to guide Phase 1 design artifacts (data model, contracts, quickstart guide).

---

## Research 1: Blazor Server File Upload Best Practices

**Question**: What are the recommended patterns for handling file uploads in Blazor Server to avoid common pitfalls?

### Decision: MemoryStream Pattern with Immediate Copy

**Rationale**: Blazor's `IBrowserFile` stream becomes disposed after the InputFile component re-renders or when the underlying connection closes. Must copy to MemoryStream immediately.

**Pattern**:
```csharp
// Extract metadata BEFORE opening stream
var fileName = selectedFile.Name;
var fileSize = selectedFile.Size;
var contentType = selectedFile.ContentType;

// Copy to MemoryStream immediately
using var memoryStream = new MemoryStream();
using (var fileStream = selectedFile.OpenReadStream(maxAllowedSize: 26_214_400)) // 25MB
{
    await fileStream.CopyToAsync(memoryStream);
}
memoryStream.Position = 0;

// Clear IBrowserFile reference to prevent reuse
selectedFile = null;
StateHasChanged();

// Now safe to use memoryStream for upload
await documentService.UploadAsync(memoryStream, fileName, contentType, ...);
```

**Key Points**:
- Use `@key` attribute on InputFile to force re-render after successful upload
- Extract all properties (Name, Size, ContentType) into local variables before stream operations
- Copy entire file to MemoryStream before any async operations
- Clear IBrowserFile reference after copy to prevent reuse errors
- Set maxAllowedSize parameter (25MB = 26,214,400 bytes) to prevent unlimited memory allocation

**Alternatives Considered**:
- Direct stream passing: Rejected - stream disposal causes ObjectDisposed exceptions
- Temporary file storage: Rejected - unnecessary complexity for training environment

---

## Research 2: Local Filesystem Security Patterns

**Question**: What is the secure approach for storing uploaded files and serving them through authorized endpoints?

### Decision: Controller-Based File Serving with Authorization

**Rationale**: Files outside wwwroot cannot be accessed via direct URLs. Must use controller endpoint with authorization checks before serving files.

**Storage Pattern**:
```csharp
// File path structure: AppData/uploads/{userId}/{projectId or "personal"}/{guid}.{extension}
var storagePath = Path.Combine(
    "AppData", "uploads",
    userId.ToString(),
    projectId.HasValue ? projectId.Value.ToString() : "personal",
    $"{Guid.NewGuid()}{Path.GetExtension(fileName)}"
);

// Create directory if needed
Directory.CreateDirectory(Path.GetDirectoryName(storagePath));

// Save file
using (var fileStream = File.Create(storagePath))
{
    await contentStream.CopyToAsync(fileStream);
}
```

**Serving Pattern**:
```csharp
[Authorize]
[HttpGet("api/documents/{documentId}/download")]
public async Task<IActionResult> DownloadDocument(int documentId)
{
    // Get document metadata from database
    var document = await documentService.GetByIdAsync(documentId, User);
    if (document == null) return NotFound();
    
    // Authorization check: user must have access to this document
    if (!await documentService.CanAccessAsync(documentId, User))
        return Forbid();
    
    // Read file from storage
    var fileStream = await fileStorageService.DownloadAsync(document.FilePath);
    
    // Return file with proper headers
    return File(fileStream, document.FileType, document.FileName);
}
```

**Security Measures**:
- Files stored outside wwwroot (not web-accessible)
- GUID-based filenames prevent path traversal attacks
- Extension whitelist validation before save
- Authorization check in controller before serving
- User identity from ASP.NET Core authentication
- IDOR protection via CanAccessAsync service method

**Alternatives Considered**:
- wwwroot storage: Rejected - bypasses authorization, security risk
- Signed URLs: Rejected - unnecessary complexity for training, more relevant for cloud storage

---

## Research 3: Entity Framework Core Relationships

**Question**: What are the optimal EF Core relationship configurations for document entities?

### Decision: Standard Foreign Key Relationships with Indexes

**Relationships**:
1. **Document → User (Uploader)**: Required, many-to-one
2. **Document → Project**: Optional, many-to-one  
3. **DocumentShare → Document**: Required, many-to-one
4. **DocumentShare → User (SharedWith)**: Required, many-to-one
5. **DocumentShare → User (SharedBy)**: Required, many-to-one

**Configuration Pattern**:
```csharp
// Document entity
public class Document
{
    public int DocumentId { get; set; }
    public int UploadedByUserId { get; set; }
    public int? ProjectId { get; set; }
    
    [ForeignKey("UploadedByUserId")]
    public virtual User UploadedBy { get; set; } = null!;
    
    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }
    
    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
}

// DocumentShare entity
public class DocumentShare
{
    public int ShareId { get; set; }
    public int DocumentId { get; set; }
    public int SharedWithUserId { get; set; }
    public int SharedByUserId { get; set; }
    
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;
    
    [ForeignKey("SharedWithUserId")]
    public virtual User SharedWithUser { get; set; } = null!;
    
    [ForeignKey("SharedByUserId")]
    public virtual User SharedByUser { get; set; } = null!;
}

// ApplicationDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Document indexes
    modelBuilder.Entity<Document>()
        .HasIndex(d => d.UploadedByUserId);
    modelBuilder.Entity<Document>()
        .HasIndex(d => d.ProjectId);
    modelBuilder.Entity<Document>()
        .HasIndex(d => d.UploadDate);
    modelBuilder.Entity<Document>()
        .HasIndex(d => d.Category);
    
    // DocumentShare indexes
    modelBuilder.Entity<DocumentShare>()
        .HasIndex(ds => ds.SharedWithUserId);
    modelBuilder.Entity<DocumentShare>()
        .HasIndex(ds => ds.DocumentId);
        
    // Prevent self-referential deletes
    modelBuilder.Entity<Document>()
        .HasOne(d => d.UploadedBy)
        .WithMany()
        .OnDelete(DeleteBehavior.Restrict);
}
```

**Index Strategy**:
- UploadedByUserId: For "My Documents" queries
- ProjectId: For "Project Documents" queries
- UploadDate: For sorting and date range filters
- Category: For category filtering
- SharedWithUserId: For "Shared with Me" queries
- DocumentId in DocumentShare: For document → shares lookups

**Query Patterns**:
```csharp
// Avoid N+1: Use Include for related data
var documents = await context.Documents
    .Include(d => d.UploadedBy)
    .Include(d => d.Project)
    .Where(d => d.UploadedByUserId == userId)
    .OrderByDescending(d => d.UploadDate)
    .ToListAsync();
```

**Alternatives Considered**:
- Composite keys: Rejected - single integer ID maintains consistency with existing User/Project entities
- Separate join table for document-project: Rejected - one-to-many relationship is simpler and sufficient

---

## Research 4: File Extension Validation

**Question**: How should file extension validation be implemented securely?

### Decision: Whitelist-Based Extension and MIME Type Validation

**Implementation**:
```csharp
private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
{
    // Extension → Allowed MIME types
    [".pdf"] = new[] { "application/pdf" },
    [".doc"] = new[] { "application/msword" },
    [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
    [".xls"] = new[] { "application/vnd.ms-excel" },
    [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
    [".ppt"] = new[] { "application/vnd.ms-powerpoint" },
    [".pptx"] = new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
    [".txt"] = new[] { "text/plain" },
    [".jpg"] = new[] { "image/jpeg" },
    [".jpeg"] = new[] { "image/jpeg" },
    [".png"] = new[] { "image/png" }
};

public static bool IsValidFile(string fileName, string contentType)
{
    var extension = Path.GetExtension(fileName).ToLowerInvariant();
    
    // Check extension is in whitelist
    if (!AllowedFileTypes.ContainsKey(extension))
        return false;
    
    // Check MIME type matches expected types for this extension
    var allowedMimeTypes = AllowedFileTypes[extension];
    return allowedMimeTypes.Contains(contentType);
}
```

**Security Considerations**:
- Whitelist approach (vs blacklist) - only explicitly allowed types
- Validate both extension AND MIME type (defense in depth)
- Case-insensitive extension comparison
- Reject any unrecognized type
- Training environment assumes safe files (no virus scanning)

---

## Research 5: File Storage Service Interface

**Question**: What interface design enables clean abstraction between local and cloud storage?

### Decision: IFileStorageService with Four Core Methods

**Interface Definition**:
```csharp
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return the storage path
    /// </summary>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, 
                             int userId, int? projectId);
    
    /// <summary>
    /// Download a file by path
    /// </summary>
    Task<Stream> DownloadAsync(string filePath);
    
    /// <summary>
    /// Delete a file by path
    /// </summary>
    Task DeleteAsync(string filePath);
    
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> ExistsAsync(string filePath);
}
```

**Local Implementation**:
```csharp
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseUploadPath = "AppData/uploads";
    
    public async Task<string> UploadAsync(Stream fileStream, string fileName, 
                                          string contentType, int userId, int? projectId)
    {
        var extension = Path.GetExtension(fileName);
        var folder = projectId.HasValue ? projectId.Value.ToString() : "personal";
        var relativePath = Path.Combine(userId.ToString(), folder, $"{Guid.NewGuid()}{extension}");
        var fullPath = Path.Combine(_baseUploadPath, relativePath);
        
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        
        using (var fileStream = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(fileStream);
        }
        
        return relativePath; // Store relative path in database
    }
    
    public async Task<Stream> DownloadAsync(string filePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        var memoryStream = new MemoryStream();
        using (var fileStream = File.OpenRead(fullPath))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }
    
    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
    
    public Task<bool> ExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}
```

**Cloud Migration Path**:
```csharp
// Future: AzureBlobStorageService implements same IFileStorageService
// Swap in Program.cs: services.AddScoped<IFileStorageService, AzureBlobStorageService>();
// FilePath column works for both (local relative paths or blob names)
// Zero changes to DocumentService, controllers, or UI
```

---

## Summary

All technical decisions resolved:

1. ✅ **File Upload**: MemoryStream pattern with immediate copy prevents stream disposal issues
2. ✅ **Security**: Controller-based serving with authorization checks, files outside wwwroot
3. ✅ **Data Model**: Standard EF Core relationships with appropriate indexes for query performance
4. ✅ **Validation**: Whitelist-based extension + MIME type validation for security
5. ✅ **Abstraction**: IFileStorageService interface enables seamless cloud migration

**No NEEDS CLARIFICATION markers remain** - All design decisions finalized. Ready for Phase 1 artifact generation.
