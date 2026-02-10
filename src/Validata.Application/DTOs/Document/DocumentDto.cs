namespace Validata.Application.DTOs.Document;

public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
    public int Type { get; set; }
    public Guid? ScreeningId { get; set; }
    public Guid? CandidateId { get; set; }
}

public class DocumentQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ScreeningId { get; set; }
    public Guid? CandidateId { get; set; }
    public int? Type { get; set; }
    public int? Status { get; set; }
}

public class DocumentResponse
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? ConfidenceScore { get; set; }
    public OcrResultResponse? OcrResult { get; set; }
    public VerificationResultResponse? VerificationResult { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class OcrResultResponse
{
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Nationality { get; set; }
}

public class VerificationResultResponse
{
    public string Status { get; set; } = string.Empty;
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Notes { get; set; }
}
