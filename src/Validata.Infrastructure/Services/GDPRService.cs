using System.Text.Json;
using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class GDPRService
{
    private readonly IGDPRRequestRepository _gdprRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<GDPRService> _logger;

    public GDPRService(
        IGDPRRequestRepository gdprRepository,
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IScreeningRequestRepository screeningRepository,
        IAuditLogRepository auditLogRepository,
        IBlobStorageService blobStorage,
        ILogger<GDPRService> logger)
    {
        _gdprRepository = gdprRepository;
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _screeningRepository = screeningRepository;
        _auditLogRepository = auditLogRepository;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<GDPRRequest> CreateDataRequestAsync(Application.DTOs.GDPR.CreateGDPRRequestRequest request, Guid requestedById)
    {
        var gdprRequest = new GDPRRequest
        {
            Id = Guid.NewGuid(),
            CandidateId = request.CandidateId,
            RequestType = request.RequestType,
            Status = (int)GDPRRequestStatus.Pending,
            Reason = request.Reason,
            RequestedAt = DateTime.UtcNow
        };

        await _gdprRepository.AddAsync(gdprRequest);
        await _gdprRepository.SaveChangesAsync();

        await LogAuditAsync("GDPR_REQUEST_CREATED", requestedById, gdprRequest.CandidateId,
            $"GDPR request {gdprRequest.Id} created for candidate {gdprRequest.CandidateId}");

        _logger.LogInformation("GDPR request {RequestId} created for candidate {CandidateId}",
            gdprRequest.Id, gdprRequest.CandidateId);

        return gdprRequest;
    }

    public async Task<GDPRRequest?> GetRequestByIdAsync(Guid id)
    {
        return await _gdprRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<GDPRRequest>> GetRequestsByCandidateAsync(Guid candidateId)
    {
        return await _gdprRepository.GetByCandidateIdAsync(candidateId);
    }

    public async Task<IEnumerable<GDPRRequest>> GetRequestsByStatusAsync(int status)
    {
        return await _gdprRepository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<GDPRRequest>> GetAllRequestsAsync(int page, int pageSize)
    {
        var all = await _gdprRepository.GetAllAsync();
        return all.OrderByDescending(r => r.RequestedAt)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize);
    }

    public async Task<GDPRRequest> ProcessRequestAsync(Guid requestId, Guid processedById)
    {
        var request = await _gdprRepository.GetByIdAsync(requestId);
        if (request == null)
            throw new InvalidOperationException($"GDPR request {requestId} not found");

        request.Status = (int)GDPRRequestStatus.Processing;
        _gdprRepository.Update(request);
        await _gdprRepository.SaveChangesAsync();

        await LogAuditAsync("GDPR_REQUEST_PROCESSING", processedById, request.CandidateId,
            $"GDPR request {requestId} is being processed");

        return request;
    }

    public async Task<GDPRRequest> CompleteRequestAsync(Guid requestId, string result, Guid completedById)
    {
        var request = await _gdprRepository.GetByIdAsync(requestId);
        if (request == null)
            throw new InvalidOperationException($"GDPR request {requestId} not found");

        request.Status = (int)GDPRRequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        request.CompletedById = completedById;
        request.Result = result;

        _gdprRepository.Update(request);
        await _gdprRepository.SaveChangesAsync();

        await LogAuditAsync("GDPR_REQUEST_COMPLETED", completedById, request.CandidateId,
            $"GDPR request {requestId} completed with result: {result}");

        _logger.LogInformation("GDPR request {RequestId} completed", requestId);

        return request;
    }

    public async Task<GDPRRequest> RejectRequestAsync(Guid requestId, string reason, Guid rejectedById)
    {
        var request = await _gdprRepository.GetByIdAsync(requestId);
        if (request == null)
            throw new InvalidOperationException($"GDPR request {requestId} not found");

        request.Status = (int)GDPRRequestStatus.Rejected;
        request.CompletedAt = DateTime.UtcNow;
        request.CompletedById = rejectedById;
        request.Result = reason;

        _gdprRepository.Update(request);
        await _gdprRepository.SaveChangesAsync();

        await LogAuditAsync("GDPR_REQUEST_REJECTED", rejectedById, request.CandidateId,
            $"GDPR request {requestId} rejected: {reason}");

        return request;
    }

    public async Task<string> ExportCandidateDataAsync(Guid candidateId)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new InvalidOperationException($"Candidate {candidateId} not found");

        var exportData = new
        {
            ExportDate = DateTime.UtcNow,
            CandidateId = candidateId,
            PersonalInfo = new
            {
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.DateOfBirth,
                candidate.Nationality,
                candidate.Address,
                candidate.ExternalId,
                candidate.CreatedAt
            },
            Documents = await GetDocumentsDataAsync(candidateId),
            Screenings = await GetScreeningsDataAsync(candidateId),
            ConsentRecords = await GetConsentRecordsAsync(candidateId),
            AuditHistory = await GetAuditHistoryAsync(candidateId)
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task<bool> DeleteCandidateDataAsync(Guid candidateId, Guid deletedById)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            return false;

        var documents = await _documentRepository.GetByCandidateIdAsync(candidateId);
        foreach (var doc in documents)
        {
            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                await _blobStorage.DeleteFileAsync(doc.FilePath);
            }
            _documentRepository.Delete(doc);
        }

        var screenings = await _screeningRepository.GetByCandidateIdAsync(candidateId);
        foreach (var screening in screenings)
        {
            _screeningRepository.Delete(screening);
        }

        _candidateRepository.Delete(candidate);
        await _candidateRepository.SaveChangesAsync();

        await LogAuditAsync("CANDIDATE_DATA_DELETED", deletedById, candidateId,
            $"All data deleted for candidate {candidateId}");

        _logger.LogInformation("All data deleted for candidate {CandidateId}", candidateId);

        return true;
    }

    public async Task<GDPRConsent> RecordConsentAsync(Guid candidateId, string consentType, bool granted, string? details = null)
    {
        var consent = new GDPRConsent
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            ConsentType = consentType,
            Granted = granted,
            Details = details,
            GrantedAt = DateTime.UtcNow,
            IpAddress = "N/A",
            UserAgent = "System"
        };

        await _auditLogRepository.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = $"CONSENT_{consentType}",
            UserId = candidateId,
            EntityType = "Consent",
            EntityId = consent.Id,
            NewValue = $"Consent {consentType}: {granted}",
            Timestamp = DateTime.UtcNow
        });

        return consent;
    }

    public async Task<IEnumerable<GDPRConsent>> GetConsentsAsync(Guid candidateId)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(candidateId);
        return logs
            .Where(l => l.Action.StartsWith("CONSENT_"))
            .Select(l => new GDPRConsent
            {
                Id = l.EntityId ?? Guid.Empty,
                CandidateId = candidateId,
                ConsentType = l.Action.Replace("CONSENT_", ""),
                Granted = true,
                GrantedAt = l.Timestamp,
                Details = l.NewValue
            });
    }

    private async Task<List<object>> GetDocumentsDataAsync(Guid candidateId)
    {
        var documents = await _documentRepository.GetByCandidateIdAsync(candidateId);
        return documents.Select(d => new
        {
            d.Id,
            d.Type,
            d.FileName,
            d.CreatedAt,
            d.Status,
            d.VerificationResult
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetScreeningsDataAsync(Guid candidateId)
    {
        var screenings = await _screeningRepository.GetByCandidateIdAsync(candidateId);
        return screenings.Select(s => new
        {
            s.Id,
            s.Status,
            s.Priority,
            s.CreatedAt,
            s.CompletedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetConsentRecordsAsync(Guid candidateId)
    {
        var consents = await GetConsentsAsync(candidateId);
        return consents.Select(c => new
        {
            c.ConsentType,
            c.Granted,
            c.GrantedAt,
            c.Details
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetAuditHistoryAsync(Guid candidateId)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(candidateId);
        return logs.OrderByDescending(l => l.Timestamp).Select(l => new
        {
            l.Action,
            l.Timestamp,
            l.NewValue
        }).Cast<object>().ToList();
    }

    private async Task LogAuditAsync(string action, Guid userId, Guid? entityId, string details)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            UserId = userId,
            EntityType = "GDPRRequest",
            EntityId = entityId,
            NewValue = details,
            Timestamp = DateTime.UtcNow
        });
        await _auditLogRepository.SaveChangesAsync();
    }
}

public class GDPRConsent
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime GrantedAt { get; set; }
    public string? Details { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
