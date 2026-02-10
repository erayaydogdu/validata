using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class SlaMonitoringService
{
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<SlaMonitoringService> _logger;

    public SlaMonitoringService(
        IScreeningRequestRepository screeningRepository,
        INotificationRepository notificationRepository,
        ILogger<SlaMonitoringService> logger)
    {
        _screeningRepository = screeningRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ScreeningRequest>> GetOverdueScreeningsAsync()
    {
        var allScreenings = await _screeningRepository.GetAllAsync();
        return allScreenings
            .Where(s => s.Status == (int)ScreeningStatus.InProgress &&
                        s.DueDate.HasValue &&
                        s.DueDate.Value < DateTime.UtcNow);
    }

    public async Task<IEnumerable<ScreeningRequest>> GetAtRiskScreeningsAsync(int hoursThreshold = 24)
    {
        var allScreenings = await _screeningRepository.GetAllAsync();
        var threshold = DateTime.UtcNow.AddHours(hoursThreshold);
        
        return allScreenings
            .Where(s => s.Status == (int)ScreeningStatus.InProgress &&
                        s.DueDate.HasValue &&
                        s.DueDate.Value <= threshold &&
                        s.DueDate.Value > DateTime.UtcNow);
    }

    public async Task<int> GetComplianceRateAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var allScreenings = await _screeningRepository.GetAllAsync();
        
        var recentScreenings = allScreenings
            .Where(s => s.CompletedAt.HasValue && s.CompletedAt.Value >= startDate)
            .ToList();

        if (!recentScreenings.Any()) return 100;

        var onTime = recentScreenings.Count(s => 
            s.CompletedAt.HasValue && 
            s.DueDate.HasValue && 
            s.CompletedAt.Value <= s.DueDate.Value);

        return (int)((double)onTime / recentScreenings.Count() * 100);
    }

    public async Task<double> GetAverageCompletionTimeAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var completedScreenings = (await _screeningRepository.GetAllAsync())
            .Where(s => s.CompletedAt.HasValue && 
                       s.CompletedAt.Value >= startDate &&
                       s.CreatedAt >= startDate)
            .ToList();

        if (!completedScreenings.Any()) return 0;

        var totalHours = completedScreenings
            .Sum(s => (s.CompletedAt!.Value - s.CreatedAt).TotalHours);

        return Math.Round(totalHours / completedScreenings.Count(), 1);
    }

    public async Task<SlaSummary> GetSlaSummaryAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var allScreenings = await _screeningRepository.GetAllAsync();
        
        var periodScreenings = allScreenings
            .Where(s => s.CreatedAt >= startDate)
            .ToList();

        var completed = periodScreenings
            .Where(s => s.Status == (int)ScreeningStatus.Completed)
            .ToList();

        var onTime = completed
            .Where(s => s.DueDate.HasValue && s.CompletedAt!.Value <= s.DueDate.Value)
            .Count();

        var overdue = periodScreenings
            .Where(s => s.Status == (int)ScreeningStatus.InProgress &&
                       s.DueDate.HasValue &&
                       s.DueDate.Value < DateTime.UtcNow)
            .Count();

        var avgHours = completed.Any() 
            ? completed.Average(s => (s.CompletedAt!.Value - s.CreatedAt).TotalHours)
            : 0;

        return new SlaSummary
        {
            PeriodDays = days,
            TotalScreenings = periodScreenings.Count,
            CompletedScreenings = completed.Count,
            OnTimeCompletions = onTime,
            OverdueScreenings = overdue,
            ComplianceRate = completed.Any() ? (int)((double)onTime / completed.Count * 100) : 100,
            AverageCompletionHours = Math.Round(avgHours, 1)
        };
    }

    public async Task CheckAndNotifyUpcomingDeadlinesAsync(int hoursBeforeDeadline = 24)
    {
        var atRisk = await GetAtRiskScreeningsAsync(hoursBeforeDeadline);

        foreach (var screening in atRisk)
        {
            if (screening.AssignedVerifierId.HasValue)
            {
                var hoursLeft = (screening.DueDate!.Value - DateTime.UtcNow).TotalHours;

                await CreateNotificationAsync(
                    screening.AssignedVerifierId.Value,
                    "Screening Deadline Approaching",
                    $"Screening {screening.Id} is due in {Math.Round(hoursLeft)} hours.",
                    "Urgent"
                );
            }

            _logger.LogWarning("Screening {ScreeningId} is at risk of missing deadline {DueDate}", 
                screening.Id, screening.DueDate);
        }
    }

    private async Task CreateNotificationAsync(Guid userId, string title, string message, string type)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = (int)NotificationType.SlaWarning,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();
    }
}

public class SlaSummary
{
    public int PeriodDays { get; set; }
    public int TotalScreenings { get; set; }
    public int CompletedScreenings { get; set; }
    public int OnTimeCompletions { get; set; }
    public int OverdueScreenings { get; set; }
    public int ComplianceRate { get; set; }
    public double AverageCompletionHours { get; set; }
}

public enum NotificationType
{
    SlaWarning = 1,
    ScreeningCompleted = 2,
    DocumentUploaded = 3,
    VerificationRequired = 4,
    General = 5
}
