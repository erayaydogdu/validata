using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Validata.Core.Enums;
using Validata.Infrastructure.Services;

namespace Validata.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportingService _reportingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        ReportingService reportingService,
        ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    [HttpGet("kpi")]
    public async Task<IActionResult> GetKpiSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var summary = await _reportingService.GetKpiSummaryAsync(fromDate, toDate);
        return Ok(summary);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _reportingService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetScreeningTrends([FromQuery] int days = 30)
    {
        var trends = await _reportingService.GetScreeningTrendsAsync(days);
        return Ok(trends);
    }

    [HttpGet("verification")]
    public async Task<IActionResult> GetVerificationBreakdown()
    {
        var breakdown = await _reportingService.GetVerificationBreakdownAsync();
        return Ok(breakdown);
    }

    [HttpGet("sla")]
    public async Task<IActionResult> GetSlaReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var report = await _reportingService.GetSlaReportAsync(fromDate, toDate);
        return Ok(report);
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport(
        [FromQuery] ReportType type,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string format = "json")
    {
        var report = await _reportingService.GenerateReportAsync(type, fromDate, toDate, format);

        if (format == "json")
        {
            return File(
                System.Text.Encoding.UTF8.GetBytes(report.Content),
                "application/json",
                report.FileName);
        }

        return File(
            System.Text.Encoding.UTF8.GetBytes(report.Content),
            "text/csv",
            report.FileName);
    }

    [HttpGet("export/kpi")]
    public async Task<IActionResult> ExportKpi([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var report = await _reportingService.GenerateReportAsync(
            ReportType.KpiSummary,
            fromDate ?? DateTime.UtcNow.AddMonths(-1),
            toDate ?? DateTime.UtcNow,
            "csv");

        return File(
            System.Text.Encoding.UTF8.GetBytes(report.Content),
            "text/csv",
            report.FileName);
    }

    [HttpGet("export/sla")]
    public async Task<IActionResult> ExportSla([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var report = await _reportingService.GenerateReportAsync(
            ReportType.SlaCompliance,
            fromDate ?? DateTime.UtcNow.AddMonths(-1),
            toDate ?? DateTime.UtcNow,
            "csv");

        return File(
            System.Text.Encoding.UTF8.GetBytes(report.Content),
            "text/csv",
            report.FileName);
    }
}
