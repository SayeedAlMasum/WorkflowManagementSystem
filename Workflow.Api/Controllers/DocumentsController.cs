using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a document
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file, string? mimeType = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not authenticated" });

            // Use provided mimeType or fall back to file.ContentType
            var actualMimeType = !string.IsNullOrEmpty(mimeType) ? mimeType : file.ContentType;

            var createDto = new CreateDocumentDto
            {
                FileName = file.FileName,
                MimeType = actualMimeType,
                Content = file.OpenReadStream()
            };

            var result = await _documentService.UploadAsync(createDto, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new { Message = "An error occurred while uploading the document" });
        }
    }

    /// <summary>
    /// Get all documents or filter by uploader
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? uploaderId = null)
    {
        try
        {
            var documents = await _documentService.ListAsync(uploaderId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            return StatusCode(500, new { Message = "An error occurred while retrieving documents" });
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var doc = await _documentService.GetByIdAsync(id);
            return Ok(doc);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Get my uploaded documents
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User not authenticated" });

            var documents = await _documentService.ListAsync(userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user documents");
            return StatusCode(500, new { Message = "An error occurred while retrieving documents" });
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _documentService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the document" });
        }
    }
}
