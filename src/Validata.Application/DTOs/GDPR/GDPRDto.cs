namespace Validata.Application.DTOs.GDPR;

public class CreateGDPRRequestRequest
{
    public int RequestType { get; set; }
    public Guid CandidateId { get; set; }
    public string? Reason { get; set; }
}

public class GDPRRequestResponse
{
    public Guid Id { get; set; }
    public int RequestType { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EstimatedCompletionDays { get; set; }
    public bool WillReceiveEmail { get; set; }
    public string? DownloadUrl { get; set; }
}

public class GDPRRequestQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Status { get; set; }
    public Guid? CandidateId { get; set; }
}
