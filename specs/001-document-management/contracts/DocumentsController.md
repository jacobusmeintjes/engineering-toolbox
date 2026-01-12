# API Contract: DocumentsController

**Feature**: Document Upload and Management  
**Controller**: `DocumentsController`  
**Base Route**: `/api/documents`  
**Authentication**: Required (all endpoints)

## Overview

REST API endpoints for document file operations (download, preview) that require controller-based serving with authorization checks. Note: Upload and metadata operations use DocumentService directly from Blazor pages, not REST API.

---

## Endpoints

### GET /api/documents/{documentId}/download

Downloads a document file.

**Authorization**: User must have access to the document (owner, project member, or share recipient)

**Route Parameters**:
- `documentId` (int, required) - Document identifier

**Response**:
- **200 OK** - File download with proper headers
  - Content-Type: Document's FileType (MIME type)
  - Content-Disposition: `attachment; filename="{FileName}"`
  - Body: File stream
- **401 Unauthorized** - User not authenticated
- **403 Forbidden** - User does not have access to this document
- **404 Not Found** - Document does not exist

**Example Request**:
```http
GET /api/documents/42/download HTTP/1.1
Host: localhost:5000
Cookie: .AspNetCore.Cookies=...
```

**Example Response (Success)**:
```http
HTTP/1.1 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename="Q4_Project_Plan.pdf"
Content-Length: 2457600

[Binary file content]
```

**Example Response (Forbidden)**:
```http
HTTP/1.1 403 Forbidden
```

**Implementation Notes**:
- Call `DocumentService.CanAccessAsync(documentId, User)` for authorization
- Use `FileStorageService.DownloadAsync(filePath)` to retrieve file
- Return `File(stream, contentType, fileName)` with proper headers
- Handle FileNotFoundException → 404 Not Found

---

### GET /api/documents/{documentId}/preview

Previews a document in the browser (PDF, images).

**Authorization**: User must have access to the document

**Route Parameters**:
- `documentId` (int, required) - Document identifier

**Response**:
- **200 OK** - File inline display with proper headers
  - Content-Type: Document's FileType (MIME type)
  - Content-Disposition: `inline; filename="{FileName}"`
  - Body: File stream
- **400 Bad Request** - File type not supported for preview
- **401 Unauthorized** - User not authenticated
- **403 Forbidden** - User does not have access to this document
- **404 Not Found** - Document does not exist

**Supported Preview Types**:
- PDF: `application/pdf`
- JPEG: `image/jpeg`
- PNG: `image/png`

**Example Request**:
```http
GET /api/documents/42/preview HTTP/1.1
Host: localhost:5000
Cookie: .AspNetCore.Cookies=...
```

**Example Response (Success)**:
```http
HTTP/1.1 200 OK
Content-Type: application/pdf
Content-Disposition: inline; filename="Q4_Project_Plan.pdf"
Content-Length: 2457600

[Binary file content - displays in browser]
```

**Example Response (Unsupported Type)**:
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "error": "Preview not supported for this file type",
  "supportedTypes": ["application/pdf", "image/jpeg", "image/png"]
}
```

**Implementation Notes**:
- Check FileType against preview whitelist: PDF, JPEG, PNG
- Use `Content-Disposition: inline` (vs `attachment`) for browser display
- Same authorization logic as download endpoint
- Return 400 Bad Request if file type cannot be previewed

---

## Security

**Authorization Strategy**:

User can access a document if:
1. They are the uploader (`UploadedByUserId == User.UserId`), OR
2. Document is in a project where they are a member, OR
3. Document has been explicitly shared with them (DocumentShare record exists)

**IDOR Protection**:
- All endpoints verify user identity from authenticated claims
- Authorization check before any file operation
- DocumentId from route parameter validated against user access rights
- Return 403 Forbidden (not 404) to prevent information leakage about document existence

**File Security**:
- Files stored outside wwwroot (not web-accessible)
- GUID-based filenames prevent path traversal
- Extension whitelist validation at upload time
- No direct file URL access - must use authorized endpoints

---

## Error Handling

**Standard Error Responses**:

```json
{
  "error": "Error message here",
  "details": "Optional additional context"
}
```

**Error Codes**:
- 400: Invalid request (unsupported preview type, invalid parameters)
- 401: Authentication required
- 403: Authorization failed (user lacks access)
- 404: Document not found
- 500: Server error (file system error, database error)

---

## Implementation Example

```csharp
[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;
    
    [HttpGet("{documentId}/download")]
    public async Task<IActionResult> DownloadDocument(int documentId)
    {
        // Get document metadata
        var document = await _documentService.GetByIdAsync(documentId);
        if (document == null)
            return NotFound();
        
        // Authorization check
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (!await _documentService.CanAccessAsync(documentId, userId))
            return Forbid();
        
        try
        {
            // Get file from storage
            var fileStream = await _fileStorageService.DownloadAsync(document.FilePath);
            
            // Return file with download headers
            return File(fileStream, document.FileType, document.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found in storage" });
        }
    }
    
    [HttpGet("{documentId}/preview")]
    public async Task<IActionResult> PreviewDocument(int documentId)
    {
        // Check if file type supports preview
        var previewTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
        
        var document = await _documentService.GetByIdAsync(documentId);
        if (document == null)
            return NotFound();
        
        if (!previewTypes.Contains(document.FileType))
        {
            return BadRequest(new 
            { 
                error = "Preview not supported for this file type",
                supportedTypes = previewTypes
            });
        }
        
        // Authorization check
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (!await _documentService.CanAccessAsync(documentId, userId))
            return Forbid();
        
        try
        {
            // Get file from storage
            var fileStream = await _fileStorageService.DownloadAsync(document.FilePath);
            
            // Return file with inline display headers
            return File(fileStream, document.FileType, document.FileName, enableRangeProcessing: true);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found in storage" });
        }
    }
}
```

---

## Testing Checklist

- [ ] Download returns correct file with proper headers
- [ ] Download enforces authorization (403 for unauthorized users)
- [ ] Download returns 404 for non-existent documents
- [ ] Preview displays PDF in browser
- [ ] Preview displays images in browser
- [ ] Preview returns 400 for unsupported file types (e.g., .docx)
- [ ] Preview enforces same authorization as download
- [ ] Unauthenticated requests return 401
- [ ] File paths with special characters handled correctly
- [ ] Large files (25MB) download successfully without timeout

---

## Notes

**Why Controller Endpoints?**
- Files outside wwwroot cannot be served directly via URL
- Controller enables authorization checks before file access
- Prevents unauthorized users from guessing file paths
- Industry-standard pattern for secure file serving

**Why Not REST API for Upload?**
- Blazor Server's InputFile component integrates directly with C# code
- No need for multipart/form-data HTTP endpoint complexity
- DocumentService called directly from Razor page code-behind
- Simpler and more maintainable for Blazor Server architecture

**Future Enhancements** (out of scope):
- Range requests for partial file downloads (video streaming)
- Thumbnail generation for image previews
- Signed URLs with expiration (relevant for cloud storage)
- Batch download (multiple files as ZIP)
