using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class DocumentProcessingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        ILogger<DocumentProcessingService> logger)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Document> ProcessUploadedDocumentAsync(
        byte[] fileContent,
        string fileName,
        string contentType,
        int documentType,
        Guid? screeningId,
        Guid? candidateId,
        Guid uploadedById)
    {
        var documentId = Guid.NewGuid();
        var extension = Path.GetExtension(fileName);
        var blobPath = await _blobStorageService.UploadDocumentAsync(
            documentId,
            new MemoryStream(fileContent),
            fileName,
            contentType);

        var document = new Document
        {
            Id = documentId,
            ScreeningRequestId = screeningId,
            CandidateId = candidateId,
            Type = documentType,
            FileName = fileName,
            FilePath = blobPath,
            MimeType = contentType,
            FileSize = fileContent.Length,
            Status = (int)DocumentStatus.Uploaded,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document);
        await _documentRepository.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} uploaded successfully", documentId);

        return document;
    }

    public async Task UpdateDocumentWithOcrResultAsync(Guid documentId, string ocrResult, double confidenceScore)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        document.OcrResult = ocrResult;
        document.Status = (int)DocumentStatus.PendingReview;
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepository.Update(document);
        await _documentRepository.SaveChangesAsync();

        _logger.LogInformation("OCR result added to document {DocumentId}", documentId);
    }

    public async Task MarkDocumentAsVerifiedAsync(Guid documentId, string verifiedBy, string? notes = null)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        document.Status = (int)DocumentStatus.Verified;
        document.VerificationResult = System.Text.Json.JsonSerializer.Serialize(new
        {
            Status = "Verified",
            VerifiedBy = verifiedBy,
            VerifiedAt = DateTime.UtcNow,
            Notes = notes
        });
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepository.Update(document);
        await _documentRepository.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} marked as verified by {VerifiedBy}", documentId, verifiedBy);
    }

    public async Task MarkDocumentAsRejectedAsync(Guid documentId, string rejectedBy, string reason)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        document.Status = (int)DocumentStatus.Rejected;
        document.VerificationResult = System.Text.Json.JsonSerializer.Serialize(new
        {
            Status = "Rejected",
            RejectedBy = rejectedBy,
            RejectedAt = DateTime.UtcNow,
            Reason = reason
        });
        document.UpdatedAt = DateTime.UtcNow;

        _documentRepository.Update(document);
        await _documentRepository.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} rejected by {RejectedBy}: {Reason}", documentId, rejectedBy, reason);
    }

    public async Task<(byte[] Content, string FileName, string ContentType)?> GetDocumentContentAsync(Guid documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null || string.IsNullOrEmpty(document.FilePath))
        {
            return null;
        }

        var stream = await _blobStorageService.DownloadFileAsync(document.FilePath);
        if (stream == null)
        {
            return null;
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        return (ms.ToArray(), document.FileName, document.MimeType ?? "application/octet-stream");
    }

    public async Task DeleteDocumentAsync(Guid documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        if (!string.IsNullOrEmpty(document.FilePath))
        {
            await _blobStorageService.DeleteFileAsync(document.FilePath);
        }

        _documentRepository.Delete(document);
        await _documentRepository.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} deleted", documentId);
    }
}
