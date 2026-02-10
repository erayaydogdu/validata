using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Validata.Application.DTOs.GDPR;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;
using Validata.Infrastructure.Services;

#pragma warning disable IDE0005
#pragma warning disable SA1200

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/gdpr")]
[Authorize]
public class GDPRController : ControllerBase
{
    private readonly GDPRService _gdprService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GDPRController> _logger;

    public GDPRController(
        GDPRService gdprService,
        IUserRepository userRepository,
        ILogger<GDPRController> logger)
    {
        _gdprService = gdprService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] Application.DTOs.GDPR.CreateGDPRRequestRequest request)
    {
        var userId = GetCurrentUserId();
        var gdprRequest = await _gdprService.CreateDataRequestAsync(request, userId);
        
        return CreatedAtAction(nameof(GetRequest), new { id = gdprRequest.Id }, 
            MapToResponse(gdprRequest));
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetRequest(Guid id)
    {
        var request = await _gdprService.GetRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        return Ok(MapToResponse(request));
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] GDPRRequestQuery query)
    {
        IEnumerable<GDPRRequest> requests;
        
        if (query.Status.HasValue)
            requests = await _gdprService.GetRequestsByStatusAsync(query.Status.Value);
        else if (query.CandidateId.HasValue)
            requests = await _gdprService.GetRequestsByCandidateAsync(query.CandidateId.Value);
        else
            requests = await _gdprService.GetAllRequestsAsync(query.Page, query.PageSize);

        var totalCount = requests.Count();
        var pagedRequests = requests.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

        return Ok(new
        {
            Data = pagedRequests.Select(MapToResponse),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    [HttpPost("requests/{id}/process")]
    public async Task<IActionResult> ProcessRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        var request = await _gdprService.ProcessRequestAsync(id, userId);
        return Ok(MapToResponse(request));
    }

    [HttpPost("requests/{id}/complete")]
    public async Task<IActionResult> CompleteRequest(Guid id, [FromBody] CompleteGDPRRequestRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _gdprService.CompleteRequestAsync(id, request.Result, userId);
        return Ok(MapToResponse(result));
    }

    [HttpPost("requests/{id}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectGDPRRequestRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _gdprService.RejectRequestAsync(id, request.Reason, userId);
        return Ok(MapToResponse(result));
    }

    [HttpGet("export/{candidateId}")]
    public async Task<IActionResult> ExportCandidateData(Guid candidateId)
    {
        var exportData = await _gdprService.ExportCandidateDataAsync(candidateId);
        
        var fileName = $"gdpr_export_{candidateId}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        return File(System.Text.Encoding.UTF8.GetBytes(exportData), "application/json", fileName);
    }

    [HttpDelete("candidates/{candidateId}")]
    public async Task<IActionResult> DeleteCandidateData(Guid candidateId)
    {
        var userId = GetCurrentUserId();
        var result = await _gdprService.DeleteCandidateDataAsync(candidateId, userId);
        
        if (!result)
            return NotFound(new { Message = $"Candidate {candidateId} not found" });

        return Ok(new { Message = "Candidate data deleted successfully" });
    }

    [HttpPost("consent")]
    public async Task<IActionResult> RecordConsent([FromBody] RecordConsentRequest request)
    {
        var consent = await _gdprService.RecordConsentAsync(
            request.CandidateId, 
            request.ConsentType, 
            request.Granted, 
            request.Details);
        
        return Ok(new { Message = "Consent recorded successfully", ConsentId = consent.Id });
    }

    [HttpGet("consent/{candidateId}")]
    public async Task<IActionResult> GetConsents(Guid candidateId)
    {
        var consents = await _gdprService.GetConsentsAsync(candidateId);
        return Ok(consents);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID");
        return userId;
    }

    private static GDPRRequestResponse MapToResponse(GDPRRequest request)
    {
        return new GDPRRequestResponse
        {
            Id = request.Id,
            RequestType = request.RequestType,
            RequestTypeName = ((GDPRRequestType)request.RequestType).ToString(),
            Status = request.Status,
            StatusName = ((GDPRRequestStatus)request.Status).ToString(),
            RequestedAt = request.RequestedAt,
            CompletedAt = request.CompletedAt,
            EstimatedCompletionDays = request.Status == (int)GDPRRequestStatus.Pending ? 30 : 0,
            WillReceiveEmail = true,
            DownloadUrl = null
        };
    }
}

public class CompleteGDPRRequestRequest
{
    public string Result { get; set; } = string.Empty;
}

public class RejectGDPRRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RecordConsentRequest
{
    public Guid CandidateId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public string? Details { get; set; }
}
