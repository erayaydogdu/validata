using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Validata.Core.Entities;

public class Organization
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SubscriptionPlan { get; set; }

    public string? Settings { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ScreeningRequest> ScreeningRequests { get; set; } = new List<ScreeningRequest>();
}

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public int Role { get; set; }

    public Guid? OrganizationId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization? Organization { get; set; }

    public ICollection<ScreeningRequest> CreatedScreenings { get; set; } = new List<ScreeningRequest>();
    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class Candidate
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Nationality { get; set; }

    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ScreeningRequest> ScreeningRequests { get; set; } = new List<ScreeningRequest>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<GDPRRequest> GDPRRequests { get; set; } = new List<GDPRRequest>();
}

public class ScreeningRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CandidateId { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    public int Status { get; set; }

    public int Priority { get; set; } = 2;

    public DateTime? DueDate { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public Guid? AssignedVerifierId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string? Notes { get; set; }

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(CreatedById))]
    public User CreatedBy { get; set; } = null!;

    [ForeignKey(nameof(AssignedVerifierId))]
    public User? AssignedVerifier { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<VerificationStep> VerificationSteps { get; set; } = new List<VerificationStep>();
}

public class Document
{
    [Key]
    public Guid Id { get; set; }

    public Guid? ScreeningRequestId { get; set; }

    public Guid? CandidateId { get; set; }

    [Required]
    public int Type { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long? FileSize { get; set; }

    public int Status { get; set; } = 1;

    public string? OcrResult { get; set; }

    public string? VerificationResult { get; set; }

    public Guid? UploadedById { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(ScreeningRequestId))]
    public ScreeningRequest? ScreeningRequest { get; set; }

    [ForeignKey(nameof(CandidateId))]
    public Candidate? Candidate { get; set; }

    [ForeignKey(nameof(UploadedById))]
    public User? UploadedBy { get; set; }
}

public class VerificationStep
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ScreeningRequestId { get; set; }

    [Required]
    public int StepType { get; set; }

    [Required]
    public int Status { get; set; }

    public int Order { get; set; }

    public Guid? AssignedToId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }

    [ForeignKey(nameof(ScreeningRequestId))]
    public ScreeningRequest ScreeningRequest { get; set; } = null!;

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }
}

public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}

public class GDPRRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CandidateId { get; set; }

    [Required]
    public int RequestType { get; set; }

    [Required]
    public int Status { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public Guid? CompletedById { get; set; }

    public string? Result { get; set; }

    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;

    [ForeignKey(nameof(CompletedById))]
    public User? CompletedBy { get; set; }
}

public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public int Type { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
