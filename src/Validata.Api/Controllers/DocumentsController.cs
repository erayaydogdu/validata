using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Validata.Application.DTOs.Common;
using Validata.Application.DTOs.Document;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;
using Validata.Infrastructure.Services;

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly DocumentProcessingService _documentProcessingService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentRepository documentRepository,
        ICandidateRepository candidateRepository,
        DocumentProcessingService documentProcessingService,
        ILogger<DocumentsController> logger)
    {
        _documentRepository = documentRepository;
        _candidateRepository = candidateRepository;
        _documentProcessingService = documentProcessingService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var uploadedById))
            {
                return Unauthorized();
            }

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new ErrorResponse { Code = "INVALID_FILE", Message = "No file provided" });
            }

            if (request.CandidateId == null && request.ScreeningId == null)
            {
                return BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = "Either CandidateId or ScreeningId must be provided" });
            }

            Guid? candidateId = request.CandidateId;
            if (candidateId == null && request.ScreeningId.HasValue)
            {
                var screening = await _documentRepository.GetByIdAsync(request.ScreeningId.Value);
                candidateId = screening?.CandidateId;
            }

            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream);

            var document = await _documentProcessingService.ProcessUploadedDocumentAsync(
                memoryStream.ToArray(),
                request.File.FileName,
                request.File.ContentType ?? "application/octet-stream",
                request.Type,
                request.ScreeningId,
                candidateId ?? Guid.Empty,
                uploadedById);

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, new DocumentResponse
            {
                Id = document.Id,
                Type = document.Type,
                TypeName = ((DocumentType)document.Type).ToString(),
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                Status = ((DocumentStatus)document.Status).ToString(),
                CreatedAt = document.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to upload document" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] DocumentQuery query)
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync();
            var filtered = documents.AsEnumerable();

            if (query.ScreeningId.HasValue)
                filtered = filtered.Where(d => d.ScreeningRequestId == query.ScreeningId.Value);

            if (query.CandidateId.HasValue)
                filtered = filtered.Where(d => d.CandidateId == query.CandidateId.Value);

            if (query.Type.HasValue)
                filtered = filtered.Where(d => d.Type == query.Type.Value);

            if (query.Status.HasValue)
                filtered = filtered.Where(d => d.Status == query.Status.Value);

            var totalCount = filtered.Count();
            var paged = filtered
                .OrderByDescending(d => d.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var response = paged.Select(d => new DocumentResponse
            {
                Id = d.Id,
                Type = d.Type,
                TypeName = ((DocumentType)d.Type).ToString(),
                FileName = d.FileName,
                FileSize = d.FileSize,
                MimeType = d.MimeType,
                Status = ((DocumentStatus)d.Status).ToString(),
                CreatedAt = d.CreatedAt,
                ExpiresAt = d.ExpiresAt
            });

            return Ok(new PaginationResponse<DocumentResponse>
            {
                Data = response,
                Pagination = new PaginationInfo
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                    HasNextPage = query.Page * query.PageSize < totalCount,
                    HasPreviousPage = query.Page > 1
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to retrieve documents" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = "Document not found" });
            }

            return Ok(new DocumentResponse
            {
                Id = document.Id,
                Type = document.Type,
                TypeName = ((DocumentType)document.Type).ToString(),
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                Status = ((DocumentStatus)document.Status).ToString(),
                CreatedAt = document.CreatedAt,
                ExpiresAt = document.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to retrieve document" });
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        try
        {
            var result = await _documentProcessingService.GetDocumentContentAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = "Document not found" });
            }

            return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download document {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to download document" });
        }
    }

    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyDocument(Guid id, [FromBody] VerifyDocumentRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _documentProcessingService.MarkDocumentAsVerifiedAsync(id, userId, request.Notes);

            return Ok(new { Id = id, Status = "Verified" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify document {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to verify document" });
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectDocument(Guid id, [FromBody] RejectDocumentRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _documentProcessingService.MarkDocumentAsRejectedAsync(id, userId, request.Reason);

            return Ok(new { Id = id, Status = "Rejected" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject document {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to reject document" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        try
        {
            await _documentProcessingService.DeleteDocumentAsync(id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to delete document" });
        }
    }
}

public class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public int Type { get; set; }
    public Guid? ScreeningId { get; set; }
    public Guid? CandidateId { get; set; }
}

public class VerifyDocumentRequest
{
    public string? Notes { get; set; }
}

public class RejectDocumentRequest
{
    public string Reason { get; set; } = string.Empty;
}
