using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class ReportingService
{
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IVerificationStepRepository _stepRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(
        IScreeningRequestRepository screeningRepository,
        IDocumentRepository documentRepository,
        IVerificationStepRepository stepRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<ReportingService> logger)
    {
        _screeningRepository = screeningRepository;
        _documentRepository = documentRepository;
        _stepRepository = stepRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<KpiSummary> GetKpiSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var screenings = await _screeningRepository.GetAllAsync();
        var steps = await _stepRepository.GetAllAsync();

        if (fromDate.HasValue)
            screenings = screenings.Where(s => s.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            screenings = screenings.Where(s => s.CreatedAt <= toDate.Value);

        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        var recentScreenings = screenings.Where(s => s.CreatedAt >= thirtyDaysAgo);
        var completedScreenings = screenings.Where(s => s.Status == (int)ScreeningStatus.Completed);
        var pendingScreenings = screenings.Where(s => s.Status == (int)ScreeningStatus.InProgress);

        var completedSteps = steps.Where(s => s.Status == (int)VerificationStatus.Completed);
        var totalProcessingTime = completedSteps
            .Where(s => s.StartedAt.HasValue && s.CompletedAt.HasValue)
            .Sum(s => (s.CompletedAt!.Value - s.StartedAt!.Value).TotalHours);

        return new KpiSummary
        {
            TotalScreenings = screenings.Count(),
            PendingScreenings = pendingScreenings.Count(),
            CompletedScreenings = completedScreenings.Count(),
            InProgressScreenings = pendingScreenings.Count(),
            AverageCompletionTimeHours = completedScreenings.Any()
                ? Math.Round(totalProcessingTime / completedScreenings.Count(), 2)
                : 0,
            ScreeningGrowthRate = CalculateGrowthRate(recentScreenings, screenings.Count()),
            CompletionRate = screenings.Any()
                ? Math.Round((double)completedScreenings.Count() / screenings.Count() * 100, 1)
                : 0,
            TotalDocumentsProcessed = await GetDocumentCountAsync(fromDate, toDate),
            AverageSlaCompliance = await CalculateSlaComplianceAsync(screenings.ToList()),
            WeeklyChange = await CalculateWeeklyChangeAsync()
        };
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var screenings = (await _screeningRepository.GetAllAsync()).ToList();
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todayScreenings = screenings.Where(s => s.CreatedAt.Date == today);
        var weekScreenings = screenings.Where(s => s.CreatedAt >= weekStart);
        var monthScreenings = screenings.Where(s => s.CreatedAt >= monthStart);

        var completedToday = todayScreenings.Where(s => s.Status == (int)ScreeningStatus.Completed);
        var completedWeek = weekScreenings.Where(s => s.Status == (int)ScreeningStatus.Completed);

        var steps = (await _stepRepository.GetAllAsync()).ToList();
        var pendingSteps = steps.Where(s => s.Status == (int)VerificationStatus.Pending);

        return new DashboardStats
        {
            TotalCandidates = await GetCandidateCountAsync(),
            ActiveScreenings = screenings.Count(s => s.Status == (int)ScreeningStatus.InProgress),
            PendingVerification = pendingSteps.Count(),
            CompletedToday = completedToday.Count(),
            CompletedThisWeek = completedWeek.Count(),
            AverageTurnaroundDays = await CalculateAverageTurnaroundDaysAsync(),
            PendingActions = pendingSteps.Count(s => s.AssignedToId.HasValue),
            UploadsToday = await GetTodayUploadsCountAsync()
        };
    }

    public async Task<ScreeningTrends> GetScreeningTrendsAsync(int days = 30)
    {
        var screenings = await _screeningRepository.GetAllAsync();
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days);

        var dailyData = new List<DailyMetric>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayStart = date;
            var dayEnd = date.AddDays(1);

            var dayScreenings = screenings.Where(s => s.CreatedAt >= dayStart && s.CreatedAt < dayEnd);
            var dayCompleted = dayScreenings.Where(s => s.Status == (int)ScreeningStatus.Completed);

            dailyData.Add(new DailyMetric
            {
                Date = date,
                Created = dayScreenings.Count(),
                Completed = dayCompleted.Count(),
                InProgress = dayScreenings.Count(s => s.Status == (int)ScreeningStatus.InProgress)
            });
        }

        return new ScreeningTrends
        {
            Period = $"{days} days",
            DailyMetrics = dailyData,
            TotalCreated = dailyData.Sum(d => d.Created),
            TotalCompleted = dailyData.Sum(d => d.Completed),
            AverageDailyCreated = Math.Round(dailyData.Average(d => d.Created), 1),
            AverageDailyCompleted = Math.Round(dailyData.Average(d => d.Completed), 1),
            PeakDay = dailyData.OrderByDescending(d => d.Created).FirstOrDefault()?.Date,
            GrowthTrend = CalculateTrend(dailyData.Select(d => d.Created).ToList())
        };
    }

    public async Task<VerificationBreakdown> GetVerificationBreakdownAsync()
    {
        var steps = await _stepRepository.GetAllAsync();
        var screenings = await _screeningRepository.GetAllAsync();

        var breakdown = new VerificationBreakdown
        {
            TotalSteps = steps.Count(),
            Pending = steps.Count(s => s.Status == (int)VerificationStatus.Pending),
            InProgress = steps.Count(s => s.Status == (int)VerificationStatus.InProgress),
            Completed = steps.Count(s => s.Status == (int)VerificationStatus.Completed),
            Failed = steps.Count(s => s.Status == (int)VerificationStatus.Failed),
            ByType = steps
                .GroupBy(s => s.StepType)
                .Select(g => new VerificationTypeMetric
                {
                    Type = ((VerificationType)g.Key).ToString(),
                    TypeValue = g.Key,
                    Total = g.Count(),
                    Completed = g.Count(s => s.Status == (int)VerificationStatus.Completed),
                    Failed = g.Count(s => s.Status == (int)VerificationStatus.Failed),
                    AverageDurationHours = g
                        .Where(s => s.StartedAt.HasValue && s.CompletedAt.HasValue)
                        .Average(s => (s.CompletedAt!.Value - s.StartedAt!.Value).TotalHours)
                })
                .OrderByDescending(m => m.Total)
                .ToList()
        };

        breakdown.CompletionRate = breakdown.TotalSteps > 0
            ? Math.Round((double)breakdown.Completed / breakdown.TotalSteps * 100, 1)
            : 0;

        return breakdown;
    }

    public async Task<SlaReport> GetSlaReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var screenings = (await _screeningRepository.GetAllAsync()).ToList();

        if (fromDate.HasValue)
            screenings = screenings.Where(s => s.CreatedAt >= fromDate.Value).ToList();
        if (toDate.HasValue)
            screenings = screenings.Where(s => s.CreatedAt <= toDate.Value).ToList();

        var completed = screenings.Where(s => s.Status == (int)ScreeningStatus.Completed && s.CompletedAt.HasValue);
        var onTime = completed.Where(s =>
            s.DueDate.HasValue && s.CompletedAt!.Value <= s.DueDate.Value);
        var overdue = completed.Where(s =>
            s.DueDate.HasValue && s.CompletedAt!.Value > s.DueDate.Value);
        var noDueDate = completed.Where(s => !s.DueDate.HasValue);

        var avgCompletionTime = completed.Any()
            ? completed.Average(s => (s.CompletedAt!.Value - s.CreatedAt).TotalHours)
            : 0;

        return new SlaReport
        {
            Period = $"{fromDate?.ToString("yyyy-MM-dd") ?? "All"} to {toDate?.ToString("yyyy-MM-dd") ?? "All"}",
            TotalScreenings = screenings.Count(),
            Completed = completed.Count(),
            OnTime = onTime.Count(),
            Overdue = overdue.Count(),
            NoDueDate = noDueDate.Count(),
            SlaComplianceRate = completed.Any()
                ? Math.Round((double)(onTime.Count() + noDueDate.Count()) / completed.Count() * 100, 1)
                : 0,
            AverageCompletionTimeHours = Math.Round(avgCompletionTime, 1),
            AverageDaysToComplete = Math.Round(avgCompletionTime / 24, 1),
            TargetCompletionDays = 5,
            ProjectedOnTimeRate = CalculateProjectedOnTimeRate(screenings),
            BreakdownByPriority = GetPriorityBreakdown(screenings)
        };
    }

    public async Task<ReportExport> GenerateReportAsync(ReportType type, DateTime fromDate, DateTime toDate, string format = "json")
    {
        object reportData = type switch
        {
            ReportType.KpiSummary => await GetKpiSummaryAsync(fromDate, toDate),
            ReportType.ScreeningTrends => await GetScreeningTrendsAsync(30),
            ReportType.VerificationBreakdown => await GetVerificationBreakdownAsync(),
            ReportType.SlaCompliance => await GetSlaReportAsync(fromDate, toDate),
            ReportType.Dashboard => await GetDashboardStatsAsync(),
            _ => await GetKpiSummaryAsync(fromDate, toDate)
        };

        var content = JsonSerializer.Serialize(reportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var fileName = $"{type}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";

        _logger.LogInformation("Report {ReportType} generated from {FromDate} to {ToDate}", type, fromDate, toDate);

        return new ReportExport
        {
            FileName = fileName,
            Content = content,
            ContentType = format == "json" ? "application/json" : "text/csv",
            GeneratedAt = DateTime.UtcNow
        };
    }

    private double CalculateGrowthRate(IEnumerable<ScreeningRequest> recent, int total)
    {
        var recentCount = recent.Count();
        return total > 0 ? Math.Round((double)recentCount / total * 100, 1) : 0;
    }

    private async Task<double> CalculateSlaComplianceAsync(List<ScreeningRequest> screenings)
    {
        var completed = screenings.Where(s => s.Status == (int)ScreeningStatus.Completed && s.CompletedAt.HasValue);
        var onTime = completed.Where(s => !s.DueDate.HasValue || s.CompletedAt!.Value <= s.DueDate.Value);
        return completed.Any()
            ? Math.Round((double)onTime.Count() / completed.Count() * 100, 1)
            : 100;
    }

    private async Task<double> CalculateWeeklyChangeAsync()
    {
        var thisWeek = DateTime.UtcNow.AddDays(-7);
        var lastWeek = DateTime.UtcNow.AddDays(-14);
        var thisWeekCount = (await _screeningRepository.GetAllAsync())
            .Count(s => s.CreatedAt >= thisWeek);
        var lastWeekCount = (await _screeningRepository.GetAllAsync())
            .Count(s => s.CreatedAt >= lastWeek && s.CreatedAt < thisWeek);
        return lastWeekCount > 0
            ? Math.Round((double)(thisWeekCount - lastWeekCount) / lastWeekCount * 100, 1)
            : 0;
    }

    private async Task<int> GetDocumentCountAsync(DateTime? fromDate, DateTime? toDate)
    {
        var docs = await _documentRepository.GetAllAsync();
        if (fromDate.HasValue) docs = docs.Where(d => d.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) docs = docs.Where(d => d.CreatedAt <= toDate.Value);
        return docs.Count();
    }

    private async Task<int> GetCandidateCountAsync()
    {
        return (await _screeningRepository.GetAllAsync())
            .Select(s => s.CandidateId)
            .Distinct()
            .Count();
    }

    private async Task<double> CalculateAverageTurnaroundDaysAsync()
    {
        var completed = (await _screeningRepository.GetAllAsync())
            .Where(s => s.Status == (int)ScreeningStatus.Completed && s.CompletedAt.HasValue);
        return completed.Any()
            ? Math.Round(completed.Average(s => (s.CompletedAt!.Value - s.CreatedAt).TotalDays), 1)
            : 0;
    }

    private async Task<int> GetTodayUploadsCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return (await _documentRepository.GetAllAsync())
            .Count(d => d.CreatedAt.Date == today);
    }

    private string CalculateTrend(List<int> values)
    {
        if (values.Count < 2) return "stable";
        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();
        var change = (secondHalf - firstHalf) / (firstHalf > 0 ? firstHalf : 1) * 100;
        return change > 5 ? "up" : change < -5 ? "down" : "stable";
    }

    private double CalculateProjectedOnTimeRate(List<ScreeningRequest> screenings)
    {
        var inProgress = screenings.Where(s => s.Status == (int)ScreeningStatus.InProgress);
        var completed = screenings.Where(s => s.Status == (int)ScreeningStatus.Completed);
        var onTime = completed.Where(s => !s.DueDate.HasValue || s.CompletedAt!.Value <= s.DueDate.Value);
        var historicalRate = completed.Any()
            ? (double)onTime.Count() / completed.Count()
            : 0.85;
        return Math.Round(historicalRate * 100, 1);
    }

    private List<PriorityMetric> GetPriorityBreakdown(List<ScreeningRequest> screenings)
    {
        return screenings
            .GroupBy(s => s.Priority)
            .Select(g => new PriorityMetric
            {
                Priority = g.Key,
                PriorityName = ((Core.Enums.Priority)g.Key).ToString(),
                Total = g.Count(),
                Completed = g.Count(s => s.Status == (int)ScreeningStatus.Completed),
                InProgress = g.Count(s => s.Status == (int)ScreeningStatus.InProgress),
                AverageDays = g.Where(s => s.CompletedAt.HasValue)
                    .Average(s => (s.CompletedAt!.Value - s.CreatedAt).TotalDays)
            })
            .OrderByDescending(m => m.Priority)
            .ToList();
    }
}

public class KpiSummary
{
    public int TotalScreenings { get; set; }
    public int PendingScreenings { get; set; }
    public int CompletedScreenings { get; set; }
    public int InProgressScreenings { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    public double ScreeningGrowthRate { get; set; }
    public double CompletionRate { get; set; }
    public int TotalDocumentsProcessed { get; set; }
    public double AverageSlaCompliance { get; set; }
    public double WeeklyChange { get; set; }
}

public class DashboardStats
{
    public int TotalCandidates { get; set; }
    public int ActiveScreenings { get; set; }
    public int PendingVerification { get; set; }
    public int CompletedToday { get; set; }
    public int CompletedThisWeek { get; set; }
    public double AverageTurnaroundDays { get; set; }
    public int PendingActions { get; set; }
    public int UploadsToday { get; set; }
}

public class DailyMetric
{
    public DateTime Date { get; set; }
    public int Created { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
}

public class ScreeningTrends
{
    public string Period { get; set; } = string.Empty;
    public List<DailyMetric> DailyMetrics { get; set; } = new();
    public int TotalCreated { get; set; }
    public int TotalCompleted { get; set; }
    public double AverageDailyCreated { get; set; }
    public double AverageDailyCompleted { get; set; }
    public DateTime? PeakDay { get; set; }
    public string GrowthTrend { get; set; } = "stable";
}

public class VerificationBreakdown
{
    public int TotalSteps { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public double CompletionRate { get; set; }
    public List<VerificationTypeMetric> ByType { get; set; } = new();
}

public class VerificationTypeMetric
{
    public string Type { get; set; } = string.Empty;
    public int TypeValue { get; set; }
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public double AverageDurationHours { get; set; }
}

public class SlaReport
{
    public string Period { get; set; } = string.Empty;
    public int TotalScreenings { get; set; }
    public int Completed { get; set; }
    public int OnTime { get; set; }
    public int Overdue { get; set; }
    public int NoDueDate { get; set; }
    public double SlaComplianceRate { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    public double AverageDaysToComplete { get; set; }
    public int TargetCompletionDays { get; set; }
    public double ProjectedOnTimeRate { get; set; }
    public List<PriorityMetric> BreakdownByPriority { get; set; } = new();
}

public class PriorityMetric
{
    public int Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public double AverageDays { get; set; }
}

public class ReportExport
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public DateTime GeneratedAt { get; set; }
}

public enum ReportType
{
    KpiSummary = 1,
    ScreeningTrends = 2,
    VerificationBreakdown = 3,
    SlaCompliance = 4,
    Dashboard = 5
}
