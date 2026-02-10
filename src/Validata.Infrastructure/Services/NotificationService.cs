using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IScreeningRequestRepository screeningRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _screeningRepository = screeningRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task SendScreeningCreatedNotificationAsync(Guid screeningId, Guid createdById)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening == null) return;

        await CreateNotificationAsync(
            screening.AssignedVerifierId ?? createdById,
            "New Screening Created",
            $"A new screening has been created for candidate {screening.CandidateId}.",
            NotificationType.ScreeningCompleted
        );
    }

    public async Task SendDocumentUploadedNotificationAsync(Guid screeningId, Guid verifierId)
    {
        await CreateNotificationAsync(
            verifierId,
            "New Document Uploaded",
            $"A new document has been uploaded for screening {screeningId}.",
            NotificationType.DocumentUploaded
        );
    }

    public async Task SendVerificationRequiredNotificationAsync(Guid verifierId, Guid screeningId, string verificationType)
    {
        await CreateNotificationAsync(
            verifierId,
            "Verification Required",
            $"Your action is required: {verificationType} verification for screening {screeningId}.",
            NotificationType.VerificationRequired
        );
    }

    public async Task SendScreeningCompletedNotificationAsync(Guid userId, Guid screeningId)
    {
        await CreateNotificationAsync(
            userId,
            "Screening Completed",
            $"Screening {screeningId} has been completed.",
            NotificationType.ScreeningCompleted
        );
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
    {
        return await _notificationRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadByUserIdAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var unread = await GetUnreadNotificationsAsync(userId);
        return unread.Count();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _notificationRepository.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _notificationRepository.Update(notification);
        }
        await _notificationRepository.SaveChangesAsync();
    }

    private async Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = (int)type,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
    }
}
