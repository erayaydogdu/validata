using Microsoft.EntityFrameworkCore;
using Validata.Core.Entities;
using Validata.Core.Enums;
using Validata.Infrastructure.Identity;
using Validata.Infrastructure.Services;

namespace Validata.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var passwordHash = PasswordHasher.HashPassword("Password123!");
        var now = DateTime.UtcNow;

        // === Organizations ===
        var acmeId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        var techStartId = Guid.Parse("a1000000-0000-0000-0000-000000000002");

        var organizations = new[]
        {
            new Organization
            {
                Id = acmeId,
                Name = "Acme Corporation",
                SubscriptionPlan = "Premium",
                Settings = "{\"maxScreenings\": 500, \"slaHours\": 48}",
                CreatedAt = now.AddDays(-90)
            },
            new Organization
            {
                Id = techStartId,
                Name = "TechStart B.V.",
                SubscriptionPlan = "Basic",
                Settings = "{\"maxScreenings\": 50, \"slaHours\": 72}",
                CreatedAt = now.AddDays(-30)
            }
        };

        // === Users ===
        var adminId = Guid.Parse("b1000000-0000-0000-0000-000000000001");
        var hrManagerId = Guid.Parse("b1000000-0000-0000-0000-000000000002");
        var hrStaffId = Guid.Parse("b1000000-0000-0000-0000-000000000003");
        var candidateUserId = Guid.Parse("b1000000-0000-0000-0000-000000000004");
        var verifierId = Guid.Parse("b1000000-0000-0000-0000-000000000005");
        var complianceId = Guid.Parse("b1000000-0000-0000-0000-000000000006");

        var users = new[]
        {
            new User
            {
                Id = adminId,
                Email = "admin@validata.com",
                PasswordHash = passwordHash,
                FirstName = "System",
                LastName = "Admin",
                Role = (int)UserRole.Admin,
                OrganizationId = null,
                IsActive = true,
                CreatedAt = now.AddDays(-90),
                LastLoginAt = now.AddHours(-2)
            },
            new User
            {
                Id = hrManagerId,
                Email = "hr.manager@acme.com",
                PasswordHash = passwordHash,
                FirstName = "Sophie",
                LastName = "de Vries",
                Role = (int)UserRole.HRManager,
                OrganizationId = acmeId,
                IsActive = true,
                CreatedAt = now.AddDays(-85),
                LastLoginAt = now.AddHours(-5)
            },
            new User
            {
                Id = hrStaffId,
                Email = "hr.staff@acme.com",
                PasswordHash = passwordHash,
                FirstName = "Thomas",
                LastName = "Bakker",
                Role = (int)UserRole.HRStaff,
                OrganizationId = acmeId,
                IsActive = true,
                CreatedAt = now.AddDays(-80),
                LastLoginAt = now.AddDays(-1)
            },
            new User
            {
                Id = candidateUserId,
                Email = "candidate@example.com",
                PasswordHash = passwordHash,
                FirstName = "Emma",
                LastName = "Jansen",
                Role = (int)UserRole.Candidate,
                OrganizationId = null,
                IsActive = true,
                CreatedAt = now.AddDays(-20),
                LastLoginAt = now.AddDays(-2)
            },
            new User
            {
                Id = verifierId,
                Email = "verifier@validata.com",
                PasswordHash = passwordHash,
                FirstName = "Lars",
                LastName = "Müller",
                Role = (int)UserRole.Verifier,
                OrganizationId = null,
                IsActive = true,
                CreatedAt = now.AddDays(-60),
                LastLoginAt = now.AddHours(-8)
            },
            new User
            {
                Id = complianceId,
                Email = "compliance@validata.com",
                PasswordHash = passwordHash,
                FirstName = "Anna",
                LastName = "Lindberg",
                Role = (int)UserRole.ComplianceOfficer,
                OrganizationId = null,
                IsActive = true,
                CreatedAt = now.AddDays(-60),
                LastLoginAt = now.AddDays(-3)
            }
        };

        // === Candidates ===
        var candidate1Id = Guid.Parse("c1000000-0000-0000-0000-000000000001");
        var candidate2Id = Guid.Parse("c1000000-0000-0000-0000-000000000002");
        var candidate3Id = Guid.Parse("c1000000-0000-0000-0000-000000000003");
        var candidate4Id = Guid.Parse("c1000000-0000-0000-0000-000000000004");

        var candidates = new[]
        {
            new Candidate
            {
                Id = candidate1Id,
                Email = "candidate@example.com",
                FirstName = "Emma",
                LastName = "Jansen",
                DateOfBirth = new DateTime(1992, 3, 15),
                Nationality = "NL",
                Address = "Keizersgracht 123, 1015 CJ Amsterdam",
                Phone = "+31 6 1234 5678",
                ExternalId = "ACME-2024-001",
                CreatedAt = now.AddDays(-20)
            },
            new Candidate
            {
                Id = candidate2Id,
                Email = "pieter.vanderberg@email.nl",
                FirstName = "Pieter",
                LastName = "van der Berg",
                DateOfBirth = new DateTime(1988, 7, 22),
                Nationality = "NL",
                Address = "Oudegracht 45, 3511 AP Utrecht",
                Phone = "+31 6 9876 5432",
                ExternalId = "ACME-2024-002",
                CreatedAt = now.AddDays(-15)
            },
            new Candidate
            {
                Id = candidate3Id,
                Email = "maria.kowalski@email.de",
                FirstName = "Maria",
                LastName = "Kowalski",
                DateOfBirth = new DateTime(1995, 11, 8),
                Nationality = "DE",
                Address = "Friedrichstraße 67, 10117 Berlin",
                Phone = "+49 170 1234567",
                ExternalId = "ACME-2024-003",
                CreatedAt = now.AddDays(-10)
            },
            new Candidate
            {
                Id = candidate4Id,
                Email = "jean.dupont@email.fr",
                FirstName = "Jean",
                LastName = "Dupont",
                DateOfBirth = new DateTime(1990, 1, 30),
                Nationality = "FR",
                Address = "12 Rue de Rivoli, 75001 Paris",
                Phone = "+33 6 12 34 56 78",
                ExternalId = "TS-2024-001",
                CreatedAt = now.AddDays(-5)
            }
        };

        // === Screening Requests ===
        var screening1Id = Guid.Parse("d1000000-0000-0000-0000-000000000001");
        var screening2Id = Guid.Parse("d1000000-0000-0000-0000-000000000002");
        var screening3Id = Guid.Parse("d1000000-0000-0000-0000-000000000003");
        var screening4Id = Guid.Parse("d1000000-0000-0000-0000-000000000004");
        var screening5Id = Guid.Parse("d1000000-0000-0000-0000-000000000005");

        var screeningRequests = new[]
        {
            new ScreeningRequest
            {
                Id = screening1Id,
                CandidateId = candidate1Id,
                OrganizationId = acmeId,
                Status = (int)ScreeningStatus.Completed,
                Priority = (int)Priority.High,
                DueDate = now.AddDays(-5),
                CreatedById = hrManagerId,
                AssignedVerifierId = verifierId,
                CompletedAt = now.AddDays(-7),
                CreatedAt = now.AddDays(-18),
                Notes = "Senior developer position - full background check required"
            },
            new ScreeningRequest
            {
                Id = screening2Id,
                CandidateId = candidate2Id,
                OrganizationId = acmeId,
                Status = (int)ScreeningStatus.InProgress,
                Priority = (int)Priority.Normal,
                DueDate = now.AddDays(5),
                CreatedById = hrManagerId,
                AssignedVerifierId = verifierId,
                CreatedAt = now.AddDays(-12),
                Notes = "Finance department - standard screening"
            },
            new ScreeningRequest
            {
                Id = screening3Id,
                CandidateId = candidate3Id,
                OrganizationId = acmeId,
                Status = (int)ScreeningStatus.Pending,
                Priority = (int)Priority.Normal,
                DueDate = now.AddDays(14),
                CreatedById = hrStaffId,
                CreatedAt = now.AddDays(-8),
                Notes = "International hire - requires education verification"
            },
            new ScreeningRequest
            {
                Id = screening4Id,
                CandidateId = candidate1Id,
                OrganizationId = acmeId,
                Status = (int)ScreeningStatus.OnHold,
                Priority = (int)Priority.Low,
                DueDate = now.AddDays(30),
                CreatedById = hrStaffId,
                CreatedAt = now.AddDays(-5),
                Notes = "Additional reference check - waiting for candidate response"
            },
            new ScreeningRequest
            {
                Id = screening5Id,
                CandidateId = candidate4Id,
                OrganizationId = techStartId,
                Status = (int)ScreeningStatus.InProgress,
                Priority = (int)Priority.Urgent,
                DueDate = now.AddDays(2),
                CreatedById = hrManagerId,
                AssignedVerifierId = verifierId,
                CreatedAt = now.AddDays(-3),
                Notes = "Urgent CTO hire - expedite all checks"
            }
        };

        // === Documents ===
        var documents = new[]
        {
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000001"),
                ScreeningRequestId = screening1Id,
                CandidateId = candidate1Id,
                Type = (int)DocumentType.Passport,
                FileName = "emma_jansen_passport.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000001/passport.pdf",
                MimeType = "application/pdf",
                FileSize = 245_000,
                Status = (int)DocumentStatus.Verified,
                VerificationResult = "{\"valid\": true, \"expiryDate\": \"2028-03-15\"}",
                UploadedById = candidateUserId,
                ExpiresAt = new DateTime(2028, 3, 15),
                CreatedAt = now.AddDays(-17)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000002"),
                ScreeningRequestId = screening1Id,
                CandidateId = candidate1Id,
                Type = (int)DocumentType.Diploma,
                FileName = "emma_jansen_msc_cs.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000001/diploma.pdf",
                MimeType = "application/pdf",
                FileSize = 520_000,
                Status = (int)DocumentStatus.Verified,
                VerificationResult = "{\"institution\": \"TU Delft\", \"degree\": \"MSc Computer Science\", \"year\": 2016}",
                UploadedById = candidateUserId,
                CreatedAt = now.AddDays(-17)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000003"),
                ScreeningRequestId = screening2Id,
                CandidateId = candidate2Id,
                Type = (int)DocumentType.IDCard,
                FileName = "pieter_vanderberg_id.jpg",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000002/id_card.jpg",
                MimeType = "image/jpeg",
                FileSize = 180_000,
                Status = (int)DocumentStatus.PendingReview,
                UploadedById = candidateUserId,
                ExpiresAt = new DateTime(2027, 9, 1),
                CreatedAt = now.AddDays(-10)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000004"),
                ScreeningRequestId = screening2Id,
                CandidateId = candidate2Id,
                Type = (int)DocumentType.CriminalRecord,
                FileName = "pieter_vanderberg_vog.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000002/criminal_record.pdf",
                MimeType = "application/pdf",
                FileSize = 95_000,
                Status = (int)DocumentStatus.Processing,
                UploadedById = candidateUserId,
                CreatedAt = now.AddDays(-9)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000005"),
                ScreeningRequestId = screening3Id,
                CandidateId = candidate3Id,
                Type = (int)DocumentType.Diploma,
                FileName = "maria_kowalski_bsc.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000003/diploma.pdf",
                MimeType = "application/pdf",
                FileSize = 410_000,
                Status = (int)DocumentStatus.Uploaded,
                UploadedById = candidateUserId,
                CreatedAt = now.AddDays(-7)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000006"),
                ScreeningRequestId = screening3Id,
                CandidateId = candidate3Id,
                Type = (int)DocumentType.WorkHistory,
                FileName = "maria_kowalski_cv.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000003/work_history.pdf",
                MimeType = "application/pdf",
                FileSize = 320_000,
                Status = (int)DocumentStatus.Uploaded,
                UploadedById = candidateUserId,
                CreatedAt = now.AddDays(-7)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000007"),
                ScreeningRequestId = screening5Id,
                CandidateId = candidate4Id,
                Type = (int)DocumentType.Passport,
                FileName = "jean_dupont_passport.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000004/passport.pdf",
                MimeType = "application/pdf",
                FileSize = 230_000,
                Status = (int)DocumentStatus.PendingReview,
                UploadedById = candidateUserId,
                ExpiresAt = new DateTime(2029, 6, 20),
                CreatedAt = now.AddDays(-2)
            },
            new Document
            {
                Id = Guid.Parse("e1000000-0000-0000-0000-000000000008"),
                ScreeningRequestId = screening5Id,
                CandidateId = candidate4Id,
                Type = (int)DocumentType.ReferenceLetter,
                FileName = "jean_dupont_reference.pdf",
                FilePath = "/documents/c1000000-0000-0000-0000-000000000004/reference.pdf",
                MimeType = "application/pdf",
                FileSize = 150_000,
                Status = (int)DocumentStatus.Uploaded,
                UploadedById = candidateUserId,
                CreatedAt = now.AddDays(-1)
            }
        };

        // === Verification Steps ===
        var verificationSteps = new[]
        {
            // Screening 1 (Completed) - all steps completed
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000001"),
                ScreeningRequestId = screening1Id,
                StepType = (int)VerificationType.IdentityCheck,
                Status = (int)VerificationStatus.Completed,
                Order = 1,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-16),
                CompletedAt = now.AddDays(-14),
                Result = "{\"match\": true, \"confidence\": 0.98}",
                Notes = "Identity confirmed via passport verification"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000002"),
                ScreeningRequestId = screening1Id,
                StepType = (int)VerificationType.Education,
                Status = (int)VerificationStatus.Completed,
                Order = 2,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-14),
                CompletedAt = now.AddDays(-10),
                Result = "{\"verified\": true, \"institution\": \"TU Delft\"}",
                Notes = "Degree confirmed with university registrar"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000003"),
                ScreeningRequestId = screening1Id,
                StepType = (int)VerificationType.Employment,
                Status = (int)VerificationStatus.Completed,
                Order = 3,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-10),
                CompletedAt = now.AddDays(-8),
                Result = "{\"verified\": true, \"positions\": 3}",
                Notes = "All previous employers confirmed"
            },
            // Screening 2 (InProgress) - mixed statuses
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000004"),
                ScreeningRequestId = screening2Id,
                StepType = (int)VerificationType.IdentityCheck,
                Status = (int)VerificationStatus.Completed,
                Order = 1,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-8),
                CompletedAt = now.AddDays(-6),
                Result = "{\"match\": true, \"confidence\": 0.95}"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000005"),
                ScreeningRequestId = screening2Id,
                StepType = (int)VerificationType.CriminalRecord,
                Status = (int)VerificationStatus.InProgress,
                Order = 2,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-5),
                Notes = "Awaiting VOG certificate processing"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000006"),
                ScreeningRequestId = screening2Id,
                StepType = (int)VerificationType.Tax,
                Status = (int)VerificationStatus.Pending,
                Order = 3
            },
            // Screening 3 (Pending) - all pending
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000007"),
                ScreeningRequestId = screening3Id,
                StepType = (int)VerificationType.Education,
                Status = (int)VerificationStatus.Pending,
                Order = 1,
                Notes = "German university degree - may need apostille"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000008"),
                ScreeningRequestId = screening3Id,
                StepType = (int)VerificationType.Employment,
                Status = (int)VerificationStatus.Pending,
                Order = 2
            },
            // Screening 5 (InProgress/Urgent)
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000009"),
                ScreeningRequestId = screening5Id,
                StepType = (int)VerificationType.IdentityCheck,
                Status = (int)VerificationStatus.InProgress,
                Order = 1,
                AssignedToId = verifierId,
                StartedAt = now.AddDays(-1),
                Notes = "Urgent - expedite passport verification"
            },
            new VerificationStep
            {
                Id = Guid.Parse("f1000000-0000-0000-0000-000000000010"),
                ScreeningRequestId = screening5Id,
                StepType = (int)VerificationType.Reference,
                Status = (int)VerificationStatus.Pending,
                Order = 2,
                Notes = "Contact references after identity is confirmed"
            }
        };

        // === Audit Logs ===
        var auditLogs = new[]
        {
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000001"),
                UserId = adminId,
                Action = "User.Login",
                EntityType = "User",
                EntityId = adminId,
                IpAddress = "10.0.0.1",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                Timestamp = now.AddDays(-10)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000002"),
                UserId = hrManagerId,
                Action = "ScreeningRequest.Create",
                EntityType = "ScreeningRequest",
                EntityId = screening1Id,
                NewValue = "{\"candidateId\": \"c1000000-0000-0000-0000-000000000001\", \"priority\": \"High\"}",
                IpAddress = "10.0.1.50",
                UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)",
                Timestamp = now.AddDays(-18)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000003"),
                UserId = candidateUserId,
                Action = "Document.Upload",
                EntityType = "Document",
                EntityId = Guid.Parse("e1000000-0000-0000-0000-000000000001"),
                NewValue = "{\"fileName\": \"emma_jansen_passport.pdf\", \"type\": \"Passport\"}",
                IpAddress = "82.168.10.25",
                UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0)",
                Timestamp = now.AddDays(-17)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000004"),
                UserId = verifierId,
                Action = "VerificationStep.Complete",
                EntityType = "VerificationStep",
                EntityId = Guid.Parse("f1000000-0000-0000-0000-000000000001"),
                OldValue = "{\"status\": \"InProgress\"}",
                NewValue = "{\"status\": \"Completed\", \"result\": {\"match\": true}}",
                IpAddress = "10.0.0.5",
                Timestamp = now.AddDays(-14)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000005"),
                UserId = verifierId,
                Action = "ScreeningRequest.Complete",
                EntityType = "ScreeningRequest",
                EntityId = screening1Id,
                OldValue = "{\"status\": \"InProgress\"}",
                NewValue = "{\"status\": \"Completed\"}",
                IpAddress = "10.0.0.5",
                Timestamp = now.AddDays(-7)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000006"),
                UserId = hrManagerId,
                Action = "User.Login",
                EntityType = "User",
                EntityId = hrManagerId,
                IpAddress = "10.0.1.50",
                Timestamp = now.AddHours(-5)
            },
            new AuditLog
            {
                Id = Guid.Parse("a2000000-0000-0000-0000-000000000007"),
                UserId = complianceId,
                Action = "GDPRRequest.Process",
                EntityType = "GDPRRequest",
                EntityId = Guid.Parse("b2000000-0000-0000-0000-000000000001"),
                NewValue = "{\"type\": \"Export\", \"status\": \"Completed\"}",
                IpAddress = "10.0.0.10",
                Timestamp = now.AddDays(-2)
            }
        };

        // === Notifications ===
        var notifications = new[]
        {
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000001"),
                UserId = hrManagerId,
                Type = (int)NotificationType.ScreeningCompleted,
                Title = "Screening Completed",
                Message = "Background screening for Emma Jansen has been completed successfully.",
                IsRead = true,
                CreatedAt = now.AddDays(-7)
            },
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000002"),
                UserId = verifierId,
                Type = (int)NotificationType.VerificationRequired,
                Title = "New Verification Assignment",
                Message = "You have been assigned to verify documents for Pieter van der Berg.",
                IsRead = true,
                CreatedAt = now.AddDays(-8)
            },
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000003"),
                UserId = verifierId,
                Type = (int)NotificationType.SlaWarning,
                Title = "Deadline Approaching",
                Message = "Screening for Jean Dupont is due in 48 hours. Please expedite remaining checks.",
                IsRead = false,
                CreatedAt = now.AddDays(-1)
            },
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000004"),
                UserId = hrManagerId,
                Type = (int)NotificationType.DocumentUploaded,
                Title = "Document Uploaded",
                Message = "Jean Dupont has uploaded a reference letter for review.",
                IsRead = false,
                CreatedAt = now.AddDays(-1)
            },
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000005"),
                UserId = candidateUserId,
                Type = (int)NotificationType.General,
                Title = "Welcome to Validata",
                Message = "Your account has been created. Please upload the required documents to proceed with your screening.",
                IsRead = true,
                CreatedAt = now.AddDays(-20)
            },
            new Notification
            {
                Id = Guid.Parse("c2000000-0000-0000-0000-000000000006"),
                UserId = complianceId,
                Type = (int)NotificationType.General,
                Title = "GDPR Request Pending",
                Message = "A new data rectification request has been submitted by Pieter van der Berg.",
                IsRead = false,
                CreatedAt = now.AddDays(-3)
            }
        };

        // === GDPR Requests ===
        var gdprRequests = new[]
        {
            new GDPRRequest
            {
                Id = Guid.Parse("b2000000-0000-0000-0000-000000000001"),
                CandidateId = candidate1Id,
                RequestType = (int)GDPRRequestType.Export,
                Status = (int)GDPRRequestStatus.Completed,
                Reason = "Candidate requested a copy of all personal data on file.",
                RequestedAt = now.AddDays(-10),
                CompletedAt = now.AddDays(-2),
                CompletedById = complianceId,
                Result = "{\"exportFile\": \"/exports/emma_jansen_data_export.zip\", \"recordCount\": 15}"
            },
            new GDPRRequest
            {
                Id = Guid.Parse("b2000000-0000-0000-0000-000000000002"),
                CandidateId = candidate2Id,
                RequestType = (int)GDPRRequestType.Rectify,
                Status = (int)GDPRRequestStatus.Pending,
                Reason = "Candidate reports incorrect date of birth in screening records.",
                RequestedAt = now.AddDays(-3)
            },
            new GDPRRequest
            {
                Id = Guid.Parse("b2000000-0000-0000-0000-000000000003"),
                CandidateId = candidate4Id,
                RequestType = (int)GDPRRequestType.Delete,
                Status = (int)GDPRRequestStatus.Processing,
                Reason = "Candidate withdrew application and requests full data deletion.",
                RequestedAt = now.AddDays(-1)
            }
        };

        // Add all entities in FK order
        context.Organizations.AddRange(organizations);
        context.Users.AddRange(users);
        context.Candidates.AddRange(candidates);
        context.ScreeningRequests.AddRange(screeningRequests);
        context.Documents.AddRange(documents);
        context.VerificationSteps.AddRange(verificationSteps);
        context.AuditLogs.AddRange(auditLogs);
        context.Notifications.AddRange(notifications);
        context.GDPRRequests.AddRange(gdprRequests);

        await context.SaveChangesAsync();
    }
}
