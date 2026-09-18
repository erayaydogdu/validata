using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Validata.Application.DTOs.Common;
using Validata.Application.DTOs.Screening;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;
using Validata.Infrastructure.Services;

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/screenings")]
[Authorize]
public class ScreeningsController : ControllerBase
{
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly IVerificationStepRepository _stepRepository;
    private readonly ScreeningWorkflowService _workflowService;
    private readonly SlaMonitoringService _slaService;
    private readonly ILogger<ScreeningsController> _logger;

    public ScreeningsController(
        IScreeningRequestRepository screeningRepository,
        IVerificationStepRepository stepRepository,
        ScreeningWorkflowService workflowService,
        SlaMonitoringService slaService,
        ILogger<ScreeningsController> logger)
    {
        _screeningRepository = screeningRepository;
        _stepRepository = stepRepository;
        _workflowService = workflowService;
        _slaService = slaService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetScreenings([FromQuery] ScreeningQuery query)
    {
        try
        {
            var screenings = await _screeningRepository.GetAllAsync();
            var filtered = screenings.AsEnumerable();

            if (query.Status.HasValue)
                filtered = filtered.Where(s => s.Status == query.Status.Value);

            if (query.CandidateId.HasValue)
                filtered = filtered.Where(s => s.CandidateId == query.CandidateId.Value);

            if (query.FromDate.HasValue)
                filtered = filtered.Where(s => s.CreatedAt >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                filtered = filtered.Where(s => s.CreatedAt <= query.ToDate.Value);

            var totalCount = filtered.Count();
            var paged = filtered
                .OrderByDescending(s => s.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var response = paged.Select(async s => new ScreeningResponse
            {
                Id = s.Id,
                CandidateId = s.CandidateId,
                CandidateName = $"{s.Candidate?.FirstName} {s.Candidate?.LastName}",
                Status = ((ScreeningStatus)s.Status).ToString(),
                Priority = ((Priority)s.Priority).ToString(),
                CreatedAt = s.CreatedAt,
                DueDate = s.DueDate,
                CompletedAt = s.CompletedAt,
                Progress = await _workflowService.GetScreeningProgressAsync(s.Id)
            });

            var responseList = new List<ScreeningResponse>();
            foreach (var r in response)
            {
                responseList.Add(await r);
            }

            return Ok(new PaginationResponse<ScreeningResponse>
            {
                Data = responseList,
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
            _logger.LogError(ex, "Failed to get screenings");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to retrieve screenings" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetScreening(Guid id)
    {
        try
        {
            var screening = await _workflowService.GetScreeningWithStepsAsync(id);
            if (screening == null)
            {
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = "Screening not found" });
            }

            var steps = await _workflowService.GetScreeningStepsAsync(id);
            var progress = await _workflowService.GetScreeningProgressAsync(id);

            return Ok(new ScreeningDetailResponse
            {
                Id = screening.Id,
                CandidateId = screening.CandidateId,
                CandidateName = $"{screening.Candidate?.FirstName} {screening.Candidate?.LastName}",
                Status = ((ScreeningStatus)screening.Status).ToString(),
                Priority = ((Priority)screening.Priority).ToString(),
                CreatedAt = screening.CreatedAt,
                DueDate = screening.DueDate,
                CompletedAt = screening.CompletedAt,
                Notes = screening.Notes,
                Progress = progress,
                Steps = steps.Select(st => new VerificationStepResponse
                {
                    Id = st.Id,
                    Type = ((VerificationType)st.StepType).ToString(),
                    Name = GetStepName(st.StepType),
                    Status = ((VerificationStatus)st.Status).ToString(),
                    StartedAt = st.StartedAt,
                    CompletedAt = st.CompletedAt,
                    Result = st.Result,
                    Order = st.Order
                }).OrderBy(s => s.Order)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get screening {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to retrieve screening" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateScreening([FromBody] CreateScreeningRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var createdById))
            {
                return Unauthorized();
            }

            var screening = await _workflowService.CreateScreeningAsync(
                request.CandidateId,
                request.OrganizationId ?? createdById,
                createdById,
                request.Priority,
                request.DueDate,
                request.Notes);

            return CreatedAtAction(nameof(GetScreening), new { id = screening.Id }, new
            {
                Id = screening.Id,
                Status = ((ScreeningStatus)screening.Status).ToString(),
                CreatedAt = screening.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create screening");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to create screening" });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateScreeningStatusRequest request)
    {
        try
        {
            var screening = await _screeningRepository.GetByIdAsync(id);
            if (screening == null)
            {
                return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = "Screening not found" });
            }

            screening.Status = request.Status;
            if (request.Status == (int)ScreeningStatus.Completed)
            {
                screening.CompletedAt = DateTime.UtcNow;
            }

            screening.UpdatedAt = DateTime.UtcNow;
            _screeningRepository.Update(screening);
            await _screeningRepository.SaveChangesAsync();

            return Ok(new { Id = screening.Id, Status = ((ScreeningStatus)screening.Status).ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update screening status {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to update screening status" });
        }
    }

    [HttpPut("{id}/assign-verifier")]
    public async Task<IActionResult> AssignVerifier(Guid id, [FromBody] AssignVerifierRequest request)
    {
        try
        {
            await _workflowService.AssignVerifierAsync(id, request.VerifierId);
            return Ok(new { Id = id, VerifierId = request.VerifierId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign verifier for screening {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to assign verifier" });
        }
    }

    [HttpPost("{id}/start-verification/{stepType}")]
    public async Task<IActionResult> StartVerification(Guid id, int stepType)
    {
        try
        {
            var result = await _workflowService.StartVerificationAsync(id, stepType);
            if (!result)
            {
                return BadRequest(new ErrorResponse { Code = "INVALID_STATE", Message = "Cannot start verification" });
            }

            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start verification for screening {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to start verification" });
        }
    }

    [HttpPost("{id}/complete-verification/{stepId}")]
    public async Task<IActionResult> CompleteVerification(Guid id, Guid stepId, [FromBody] CompleteVerificationRequest request)
    {
        try
        {
            var result = await _workflowService.CompleteVerificationAsync(stepId, request.Result, request.Passed, request.Notes);
            if (!result)
            {
                return BadRequest(new ErrorResponse { Code = "INVALID_STATE", Message = "Cannot complete verification" });
            }

            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete verification for screening {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to complete verification" });
        }
    }

    [HttpGet("sla/summary")]
    public async Task<IActionResult> GetSlaSummary([FromQuery] int days = 30)
    {
        try
        {
            var summary = await _slaService.GetSlaSummaryAsync(days);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get SLA summary");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to get SLA summary" });
        }
    }

    [HttpGet("sla/overdue")]
    public async Task<IActionResult> GetOverdueScreenings()
    {
        try
        {
            var overdue = await _slaService.GetOverdueScreeningsAsync();
            return Ok(overdue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get overdue screenings");
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to get overdue screenings" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelScreening(Guid id)
    {
        try
        {
            await _workflowService.CancelScreeningAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel screening {Id}", id);
            return StatusCode(500, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "Failed to cancel screening" });
        }
    }

    private static string GetStepName(int stepType)
    {
        return stepType switch
        {
            1 => "Identity Check",
            2 => "Criminal Record Check",
            3 => "Education Verification",
            4 => "Employment Verification",
            5 => "Tax Verification",
            6 => "Reference Check",
            _ => "Unknown"
        };
    }
}

public class ScreeningQuery : PaginationQuery
{
    public int? Status { get; set; }
    public Guid? CandidateId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class PaginationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateScreeningRequest
{
    public Guid CandidateId { get; set; }
    public Guid? OrganizationId { get; set; }
    public int Priority { get; set; } = 2;
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateScreeningStatusRequest
{
    public int Status { get; set; }
}

public class AssignVerifierRequest
{
    public Guid VerifierId { get; set; }
}

public class CompleteVerificationRequest
{
    public bool Passed { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ScreeningDetailResponse : ScreeningResponse
{
    public string? Notes { get; set; }
}
