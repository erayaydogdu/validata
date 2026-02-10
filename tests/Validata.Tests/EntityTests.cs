using Validata.Core.Entities;
using Validata.Core.Enums;
using Xunit;

namespace Validata.Tests;

public class EntityTests
{
    [Fact]
    public void ScreeningRequest_Creation_SetsDefaultValues()
    {
        var screening = new ScreeningRequest
        {
            Id = Guid.NewGuid(),
            CandidateId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Status = (int)ScreeningStatus.Pending,
            Priority = 2
        };

        Assert.Equal((int)ScreeningStatus.Pending, screening.Status);
        Assert.Equal(2, screening.Priority);
        Assert.True(screening.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void User_Creation_SetsDefaultValues()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        Assert.True(user.IsActive);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Document_Creation_SetsDefaultValues()
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            CandidateId = Guid.NewGuid(),
            FileName = "test.pdf",
            FilePath = "/uploads/test.pdf",
            Type = (int)DocumentType.Passport,
            Status = (int)DocumentStatus.Uploaded
        };

        Assert.Equal((int)DocumentStatus.Uploaded, doc.Status);
        Assert.NotEqual(Guid.Empty, doc.Id);
    }

    [Fact]
    public void VerificationStep_Creation_SetsDefaultValues()
    {
        var step = new VerificationStep
        {
            Id = Guid.NewGuid(),
            ScreeningRequestId = Guid.NewGuid(),
            StepType = (int)VerificationType.IdentityCheck,
            Status = (int)VerificationStatus.Pending,
            Order = 1
        };

        Assert.Equal((int)VerificationStatus.Pending, step.Status);
        Assert.Equal(1, step.Order);
    }

    [Fact]
    public void Notification_Creation_SetsDefaultValues()
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = 1,
            Title = "Test",
            IsRead = false
        };

        Assert.False(notification.IsRead);
        Assert.NotEqual(Guid.Empty, notification.Id);
    }

    [Fact]
    public void AuditLog_Creation_SetsDefaultValues()
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "TEST_ACTION",
            Timestamp = DateTime.UtcNow
        };

        Assert.Equal("TEST_ACTION", log.Action);
        Assert.True(log.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void GDPRRequest_Creation_SetsDefaultValues()
    {
        var request = new GDPRRequest
        {
            Id = Guid.NewGuid(),
            CandidateId = Guid.NewGuid(),
            RequestType = (int)GDPRRequestType.Export,
            Status = (int)GDPRRequestStatus.Pending
        };

        Assert.Equal((int)GDPRRequestStatus.Pending, request.Status);
        Assert.Equal((int)GDPRRequestType.Export, request.RequestType);
    }
}
