namespace Validata.Application.DTOs.Screening;

public class CreateScreeningRequest
{
    public Guid CandidateId { get; set; }
    public Guid? PackageId { get; set; }
    public int Priority { get; set; } = 2;
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

public class ScreeningQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Status { get; set; }
    public Guid? CandidateId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ScreeningResponse
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Progress { get; set; }
    public IEnumerable<VerificationStepResponse> Steps { get; set; } = new List<VerificationStepResponse>();
}

public class VerificationStepResponse
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Result { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateScreeningStatusRequest
{
    public int Status { get; set; }
}

public class AssignVerifierRequest
{
    public Guid VerifierId { get; set; }
}
