using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Validata.Core.Entities;
using Validata.Core.Interfaces;

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditLogRepository auditRepository,
        ILogger<AuditController> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQuery query)
    {
        var logs = await _auditRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(query.Action))
            logs = logs.Where(l => l.Action.Contains(query.Action, StringComparison.OrdinalIgnoreCase));

        if (query.FromDate.HasValue)
            logs = logs.Where(l => l.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            logs = logs.Where(l => l.Timestamp <= query.ToDate.Value);

        if (query.UserId.HasValue)
            logs = logs.Where(l => l.UserId == query.UserId.Value);

        var totalCount = logs.Count();
        var pagedLogs = logs
            .OrderByDescending(l => l.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        return Ok(new
        {
            Data = pagedLogs.Select(MapToResponse),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAuditLogs(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _auditRepository.GetByUserIdAsync(userId);
        var pagedLogs = logs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Ok(pagedLogs.Select(MapToResponse));
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetEntityAuditLogs(string entityType, Guid entityId)
    {
        var logs = await _auditRepository.GetAllAsync();
        var entityLogs = logs
            .Where(l => l.EntityType != null && 
                        l.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) &&
                        l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp);

        return Ok(entityLogs.Select(MapToResponse));
    }

    private static AuditLogResponse MapToResponse(AuditLog log)
    {
        return new AuditLogResponse
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            Timestamp = log.Timestamp
        };
    }
}

public class AuditLogQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? UserId { get; set; }
}

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}
