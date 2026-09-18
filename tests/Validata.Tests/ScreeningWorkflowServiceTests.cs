using Microsoft.Extensions.Logging;
using Moq;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Core.Interfaces;
using Validata.Infrastructure.Services;

namespace Validata.Tests;

/// <summary>
/// Covers the step-result inspection in CompleteVerificationAsync. The previous implementation
/// read the Passed flag through JsonSerializer.Deserialize&lt;dynamic&gt;(...).Passed, which both
/// dereferenced a possibly-null value and threw RuntimeBinderException for any completed step
/// whose result was a JSON object (JsonElement has no Passed member).
/// </summary>
public class ScreeningWorkflowServiceTests
{
    private static (ScreeningWorkflowService Service, ScreeningRequest Screening) BuildService(
        Guid screeningId,
        VerificationStep target,
        params VerificationStep[] otherSteps)
    {
        var screening = new ScreeningRequest
        {
            Id = screeningId,
            CandidateId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Status = (int)ScreeningStatus.InProgress
        };

        var steps = new List<VerificationStep> { target };
        steps.AddRange(otherSteps);

        var stepRepository = new Mock<IVerificationStepRepository>();
        stepRepository.Setup(r => r.GetByIdAsync(target.Id)).ReturnsAsync(target);
        stepRepository.Setup(r => r.GetByScreeningRequestIdAsync(screeningId)).ReturnsAsync(steps);

        var screeningRepository = new Mock<IScreeningRequestRepository>();
        screeningRepository.Setup(r => r.GetByIdAsync(screeningId)).ReturnsAsync(screening);

        var service = new ScreeningWorkflowService(
            screeningRepository.Object,
            stepRepository.Object,
            new Mock<IDocumentRepository>().Object,
            new Mock<INotificationRepository>().Object,
            new Mock<ILogger<ScreeningWorkflowService>>().Object);

        return (service, screening);
    }

    private static VerificationStep InProgressStep(Guid screeningId) => new()
    {
        Id = Guid.NewGuid(),
        ScreeningRequestId = screeningId,
        StepType = 1,
        Status = (int)VerificationStatus.InProgress
    };

    private static VerificationStep CompletedStep(Guid screeningId, string result) => new()
    {
        Id = Guid.NewGuid(),
        ScreeningRequestId = screeningId,
        StepType = 2,
        Status = (int)VerificationStatus.Completed,
        Result = result,
        CompletedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task CompleteVerification_PutsScreeningOnHold_WhenASiblingStepRecordedFailure()
    {
        var screeningId = Guid.NewGuid();
        var target = InProgressStep(screeningId);
        var failed = CompletedStep(screeningId, "{\"Passed\":false,\"Result\":\"id check failed\"}");
        var (service, screening) = BuildService(screeningId, target, failed);

        var completed = await service.CompleteVerificationAsync(target.Id, "passed", true);

        Assert.True(completed);
        Assert.Equal((int)ScreeningStatus.OnHold, screening.Status);
    }

    [Fact]
    public async Task CompleteVerification_CompletesScreening_WhenNoFailuresAndNoPendingSteps()
    {
        var screeningId = Guid.NewGuid();
        var target = InProgressStep(screeningId);
        var (service, screening) = BuildService(screeningId, target);

        var completed = await service.CompleteVerificationAsync(target.Id, "passed", true);

        Assert.True(completed);
        Assert.Equal((int)ScreeningStatus.Completed, screening.Status);
    }

    [Fact]
    public async Task CompleteVerification_IgnoresUnparsableAndMissingResults()
    {
        var screeningId = Guid.NewGuid();
        var target = InProgressStep(screeningId);
        var malformed = CompletedStep(screeningId, "not json at all");
        var noResult = CompletedStep(screeningId, "{}");
        var (service, screening) = BuildService(screeningId, target, malformed, noResult);

        var completed = await service.CompleteVerificationAsync(target.Id, "passed", true);

        Assert.True(completed);
        Assert.Equal((int)ScreeningStatus.Completed, screening.Status);
    }

    [Fact]
    public async Task CompleteVerification_KeepsScreeningInProgress_WhenASiblingStepIsPending()
    {
        var screeningId = Guid.NewGuid();
        var target = InProgressStep(screeningId);
        var pending = new VerificationStep
        {
            Id = Guid.NewGuid(),
            ScreeningRequestId = screeningId,
            StepType = 3,
            Status = (int)VerificationStatus.Pending
        };
        var (service, screening) = BuildService(screeningId, target, pending);

        var completed = await service.CompleteVerificationAsync(target.Id, "passed", true);

        Assert.True(completed);
        Assert.Equal((int)ScreeningStatus.InProgress, screening.Status);
    }
}
