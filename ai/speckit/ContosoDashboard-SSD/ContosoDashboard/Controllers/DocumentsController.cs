using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContosoDashboard.Services;
using System.Security.Claims;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Downloads a document file
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (fileStream, contentType, fileName) = await _documentService.DownloadAsync(id, userId);
            
            return File(fileStream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Document not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, new { message = "Error downloading document" });
        }
    }

    /// <summary>
    /// Previews a document file inline (for PDFs, images)
    /// </summary>
    [HttpGet("{id}/preview")]
    public async Task<IActionResult> Preview(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (fileStream, contentType, fileName) = await _documentService.DownloadAsync(id, userId);
            
            // Only allow preview for certain file types
            var previewableTypes = new[] 
            { 
                "application/pdf", 
                "image/jpeg", 
                "image/png", 
                "image/gif",
                "text/plain"
            };
            
            if (!previewableTypes.Contains(contentType.ToLower()))
            {
                return BadRequest(new { message = "This file type cannot be previewed. Please download it instead." });
            }
            
            // Return file for inline display
            return File(fileStream, contentType);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Document not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing document {DocumentId}", id);
            return StatusCode(500, new { message = "Error previewing document" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in claims");
    }
}
