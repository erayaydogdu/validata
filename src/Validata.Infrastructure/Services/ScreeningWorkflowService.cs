using Microsoft.Extensions.Logging;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Services;

public class ScreeningWorkflowService
{
    private readonly IScreeningRequestRepository _screeningRepository;
    private readonly IVerificationStepRepository _stepRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<ScreeningWorkflowService> _logger;

    public ScreeningWorkflowService(
        IScreeningRequestRepository screeningRepository,
        IVerificationStepRepository stepRepository,
        IDocumentRepository documentRepository,
        INotificationRepository notificationRepository,
        ILogger<ScreeningWorkflowService> logger)
    {
        _screeningRepository = screeningRepository;
        _stepRepository = stepRepository;
        _documentRepository = documentRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<ScreeningRequest> CreateScreeningAsync(
        Guid candidateId,
        Guid organizationId,
        Guid createdById,
        int priority = 2,
        DateTime? dueDate = null,
        string? notes = null)
    {
        var screening = new ScreeningRequest
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            OrganizationId = organizationId,
            Status = (int)ScreeningStatus.Pending,
            Priority = priority,
            DueDate = dueDate ?? CalculateDefaultDueDate(priority),
            CreatedById = createdById,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        await _screeningRepository.AddAsync(screening);

        var defaultSteps = CreateDefaultVerificationSteps(screening.Id);
        foreach (var step in defaultSteps)
        {
            await _stepRepository.AddAsync(step);
        }

        await _screeningRepository.SaveChangesAsync();

        _logger.LogInformation("Screening {ScreeningId} created for candidate {CandidateId}", screening.Id, candidateId);

        return screening;
    }

    private List<VerificationStep> CreateDefaultVerificationSteps(Guid screeningId)
    {
        return new List<VerificationStep>
        {
            new VerificationStep
            {
                Id = Guid.NewGuid(),
                ScreeningRequestId = screeningId,
                StepType = (int)VerificationType.IdentityCheck,
                Status = (int)VerificationStatus.Pending,
                Order = 1
            },
            new VerificationStep
            {
                Id = Guid.NewGuid(),
                ScreeningRequestId = screeningId,
                StepType = (int)VerificationType.CriminalRecord,
                Status = (int)VerificationStatus.Pending,
                Order = 2
            },
            new VerificationStep
            {
                Id = Guid.NewGuid(),
                ScreeningRequestId = screeningId,
                StepType = (int)VerificationType.Education,
                Status = (int)VerificationStatus.Pending,
                Order = 3
            },
            new VerificationStep
            {
                Id = Guid.NewGuid(),
                ScreeningRequestId = screeningId,
                StepType = (int)VerificationType.Employment,
                Status = (int)VerificationStatus.Pending,
                Order = 4
            }
        };
    }

    private DateTime CalculateDefaultDueDate(int priority)
    {
        var hours = priority switch
        {
            (int)Priority.Urgent => 72,
            (int)Priority.High => 120,
            (int)Priority.Normal => 168,
            (int)Priority.Low => 336,
            _ => 168
        };
        return DateTime.UtcNow.AddHours(hours);
    }

    public async Task<bool> StartVerificationAsync(Guid screeningId, int stepType)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening == null || screening.Status != (int)ScreeningStatus.Pending)
        {
            return false;
        }

        var step = (await _stepRepository.GetByScreeningRequestIdAsync(screeningId))
            .FirstOrDefault(s => s.StepType == stepType && s.Status == (int)VerificationStatus.Pending);

        if (step == null)
        {
            return false;
        }

        step.Status = (int)VerificationStatus.InProgress;
        step.StartedAt = DateTime.UtcNow;

        screening.Status = (int)ScreeningStatus.InProgress;
        screening.UpdatedAt = DateTime.UtcNow;

        _screeningRepository.Update(screening);
        _stepRepository.Update(step);
        await _screeningRepository.SaveChangesAsync();

        _logger.LogInformation("Verification step {StepType} started for screening {ScreeningId}", stepType, screeningId);

        return true;
    }

    public async Task<bool> CompleteVerificationAsync(Guid stepId, string result, bool passed, string? notes = null)
    {
        var step = await _stepRepository.GetByIdAsync(stepId);
        if (step == null || step.Status != (int)VerificationStatus.InProgress)
        {
            return false;
        }

        step.Status = (int)VerificationStatus.Completed;
        step.CompletedAt = DateTime.UtcNow;
        step.Result = System.Text.Json.JsonSerializer.Serialize(new
        {
            Passed = passed,
            Result = result,
            Notes = notes,
            CompletedAt = DateTime.UtcNow
        });

        var allSteps = await _stepRepository.GetByScreeningRequestIdAsync(step.ScreeningRequestId);
        var pendingSteps = allSteps.Where(s => s.Status == (int)VerificationStatus.Pending).ToList();
        var failedSteps = allSteps.Where(s => s.Status == (int)VerificationStatus.Completed &&
            HasExplicitlyFailed(s.Result)).ToList();

        var screening = await _screeningRepository.GetByIdAsync(step.ScreeningRequestId);
        if (screening == null) return false;

        if (failedSteps.Any())
        {
            screening.Status = (int)ScreeningStatus.OnHold;
        }
        else if (!pendingSteps.Any())
        {
            screening.Status = (int)ScreeningStatus.Completed;
            screening.CompletedAt = DateTime.UtcNow;
        }

        screening.UpdatedAt = DateTime.UtcNow;

        _stepRepository.Update(step);
        _screeningRepository.Update(screening);
        await _screeningRepository.SaveChangesAsync();

        _logger.LogInformation("Verification step {StepId} completed for screening {ScreeningId}", stepId, step.ScreeningRequestId);

        return true;
    }

    /// <summary>
    /// Reads the "Passed" flag that CompleteVerificationAsync writes into a step result.
    /// Returns true only when the result explicitly records Passed = false; missing, malformed
    /// or non-object results are treated as "not a recorded failure".
    /// </summary>
    private static bool HasExplicitlyFailed(string? resultJson)
    {
        if (string.IsNullOrWhiteSpace(resultJson))
        {
            return false;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(resultJson);

            return document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object
                && document.RootElement.TryGetProperty("Passed", out var passed)
                && passed.ValueKind == System.Text.Json.JsonValueKind.False;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }

    public async Task<ScreeningRequest?> GetScreeningWithStepsAsync(Guid screeningId)
    {
        return await _screeningRepository.GetByIdAsync(screeningId);
    }

    public async Task<IEnumerable<VerificationStep>> GetScreeningStepsAsync(Guid screeningId)
    {
        return await _stepRepository.GetByScreeningRequestIdAsync(screeningId);
    }

    public async Task<int> GetScreeningProgressAsync(Guid screeningId)
    {
        var steps = await _stepRepository.GetByScreeningRequestIdAsync(screeningId);
        if (!steps.Any()) return 0;

        var completed = steps.Count(s => s.Status == (int)VerificationStatus.Completed);
        return (int)((double)completed / steps.Count() * 100);
    }

    public async Task AssignVerifierAsync(Guid screeningId, Guid verifierId)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening != null)
        {
            screening.AssignedVerifierId = verifierId;
            screening.UpdatedAt = DateTime.UtcNow;
            _screeningRepository.Update(screening);
            await _screeningRepository.SaveChangesAsync();
        }
    }

    public async Task CancelScreeningAsync(Guid screeningId)
    {
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        if (screening != null)
        {
            screening.Status = (int)ScreeningStatus.Cancelled;
            screening.UpdatedAt = DateTime.UtcNow;
            _screeningRepository.Update(screening);
            await _screeningRepository.SaveChangesAsync();
        }
    }
}
