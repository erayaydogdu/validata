# Technical Specifications: Employment Documentation Validation Platform

## 1. Executive Summary

Build an end-to-end employment screening platform supporting document validation (IDs, diplomas, work history, tax docs) for employers, employees, and third-party verifiers. GDPR-compliant, using .NET 10 backend and Angular frontend.

---

## 2. Functional Requirements

### 2.1 User Roles

| Role | Description |
|------|-------------|
| **Admin** | System configuration, user management, platform settings |
| **HR Manager** | Create screening requests, review results, manage candidates |
| **HR Staff** | Submit screening requests, upload documents |
| **Candidate** | Self-service portal, upload documents, track status |
| **Verifier** | Third-party agency (e.g., police for criminal records) |
| **Compliance Officer** | Audit logs, GDPR data access requests |

### 2.2 Core Modules

| Module | Features |
|--------|----------|
| **Identity Verification** | ID document upload, OCR extraction, liveness check (optional), manual review |
| **Education Verification** | Diploma/certificate upload, university API integration, manual verification workflow |
| **Employment Verification** | Previous employer contact, reference checks, work history validation |
| **Criminal Record Check** | Police database API integration (country-specific), manual certificate upload |
| **Tax Document Validation** | Tax return upload, government tax authority API (where available) |
| **Screening Workflow** | Multi-step pipeline, status tracking, notifications, SLA monitoring |
| **Dashboard & Reporting** | KPIs, completion rates, compliance reports, audit trails |
| **Document Management** | Secure storage, version control, expiration reminders |
| **GDPR Portal** | Data access requests, deletion requests, consent management |

---

## 3. Non-Functional Requirements

| Requirement | Specification |
|-------------|---------------|
| **Performance** | API response < 500ms, document upload < 5s |
| **Scalability** | Horizontal scaling, auto-scaling based on load |
| **Availability** | 99.5% uptime SLA |
| **Security** | TLS 1.3, AES-256 encryption at rest, role-based access control |
| **Compliance** | GDPR, ISO 27001 ready, SOC 2 audit-ready |
| **Auditability** | Immutable audit logs, all actions tracked |
| **Multi-tenancy** | Support multiple client organizations (B2B) |

---

## 4. System Architecture

### 4.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              CDN (Cloudflare/Azure CDN)                  │
└─────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      Angular SPA (Frontend)                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐   │
│  │   HR Portal │ │ Candidate   │ │ Admin       │ │ Verifier Portal │   │
│  │             │ │ Portal       │ │ Console     │ │                 │   │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API Gateway (Azure APIM / Ocelot)                │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  Rate Limiting │ Authentication │ Request Routing │ Logging      │    │
│  └──────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                                      │
           ┌──────────────────────────┼──────────────────────────┐
           ▼                          ▼                          ▼
┌──────────────────┐    ┌──────────────────────────────┐    ┌──────────────────┐
│  Identity Service │    │   Document Processing       │    │  Workflow        │
│  (.NET 10)       │    │   Service (.NET 10)          │    │  Service (.NET 10)│
└──────────────────┘    └──────────────────────────────┘    └──────────────────┘
           │                          │                          │
           ▼                          ▼                          ▼
┌──────────────────┐    ┌──────────────────────────────┐    ┌──────────────────┐
│ External APIs:   │    │  ┌────────────────────────┐  │    │  State Machine   │
│ - iDenfy         │    │  │ Document Storage       │  │    │  - Status        │
│ - Jumio          │    │  │ (Blob Storage)         │  │    │  - Transitions   │
│ - Onfido         │    │  └────────────────────────┘  │    │  - SLA           │
└──────────────────┘    │  ┌────────────────────────┐  │    └──────────────────┘
                        │  │ OCR Engine            │  │
                        │  │ (Tesseract/Azure AI)  │  │
                        │  └────────────────────────┘  │
                        │  ┌────────────────────────┐  │
                        │  │ Manual Review Queue    │  │
                        │  │ (Internal App)         │  │
                        │  └────────────────────────┘  │
                        └──────────────────────────────┘
                                       │
                                       ▼
                        ┌──────────────────────────────┐
                        │    Shared Components          │
                        │  ┌────────────────────────┐  │
                        │  │ Event Bus (RabbitMQ/   │  │
                        │  │ Azure Service Bus)     │  │
                        │  └────────────────────────┘  │
                        │  ┌────────────────────────┐  │
                        │  │ Cache (Redis)           │  │
                        │  └────────────────────────┘  │
                        │  ┌────────────────────────┐  │
                        │  │ Logging (Serilog/      │  │
                        │  │  Application Insights) │  │
                        │  └────────────────────────┘  │
                        └──────────────────────────────┘
```

### 4.2 Backend Architecture (.NET 10)

```
src/
├── Validata.Api/                    # REST API Project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ScreeningController.cs
│   │   ├── DocumentController.cs
│   │   ├── CandidateController.cs
│   │   └── ReportController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandling.cs
│   │   ├── RequestLogging.cs
│   │   └── AuditMiddleware.cs
│   └── Program.cs
│
├── Validata.Core/                    # Core Domain Layer
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Candidate.cs
│   │   ├── ScreeningRequest.cs
│   │   ├── Document.cs
│   │   ├── VerificationResult.cs
│   │   └── AuditLog.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IScreeningService.cs
│   │   └── IDocumentService.cs
│   ├── Enums/
│   │   ├── ScreeningStatus.cs
│   │   ├── DocumentType.cs
│   │   └── UserRole.cs
│   └── DomainEvents/
│       ├── ScreeningCreated.cs
│       ├── DocumentUploaded.cs
│       └── VerificationCompleted.cs
│
├── Validata.Application/             # Application Layer
│   ├── Services/
│   │   ├── ScreeningService.cs
│   │   ├── DocumentProcessingService.cs
│   │   ├── VerificationService.cs
│   │   └── NotificationService.cs
│   ├── DTOs/
│   │   ├── ScreeningDto.cs
│   │   ├── DocumentDto.cs
│   │   └── ReportDto.cs
│   ├── Validators/
│   │   ├── ScreeningRequestValidator.cs
│   │   └── DocumentValidator.cs
│   └── Behaviors/
│       └── ValidationBehavior.cs
│
├── Validata.Infrastructure/          # Infrastructure Layer
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Repositories/
│   │   │   ├── ScreeningRepository.cs
│   │   │   └── DocumentRepository.cs
│   │   └── Configurations/
│   │       ├── UserConfiguration.cs
│   │       └── ScreeningConfiguration.cs
│   ├── Services/
│   │   ├── BlobStorageService.cs
│   │   ├── OcrService.cs
│   │   ├── ExternalApiClient.cs
│   │   └── PdfGenerator.cs
│   ├── Identity/
│   │   ├── JwtTokenService.cs
│   │   └── UserClaimsPrincipalFactory.cs
│   └── Messaging/
│       ├── RabbitMqPublisher.cs
│       └── EventConsumer.cs
│
├── Validata.Integration/              # External Integrations
│   ├── Police/
│   │   ├── DutchPoliceApiClient.cs
│   │   └── SwedishPoliceApiClient.cs
│   ├── Education/
│   │   ├── DiplomaVerificationApi.cs
│   │   └── UniversityEndpoints.cs
│   └── Identity/
│       ├── IdVerificationProvider.cs
│       └── LivenessCheckService.cs
│
└── Validata.Worker/                  # Background Jobs
    ├── BackgroundServices/
    │   ├── DocumentProcessingWorker.cs
    │   ├── SlaMonitoringWorker.cs
    │   └── ExternalApiSyncWorker.cs
```

### 4.3 Frontend Architecture (Angular 19+)

```
src/
├── app/
│   ├── core/                         # Core Module (Singleton Services)
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts
│   │   │   ├── error.interceptor.ts
│   │   │   └── audit.interceptor.ts
│   │   ├── guards/
│   │   │   ├── auth.guard.ts
│   │   │   ├── role.guard.ts
│   │   │   └── tenant.guard.ts
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   ├── screening.service.ts
│   │   │   ├── document.service.ts
│   │   │   └── notification.service.ts
│   │   ├── models/
│   │   │   ├── user.model.ts
│   │   │   ├── screening.model.ts
│   │   │   └── document.model.ts
│   │   └── config/
│   │       └── app.config.ts
│   │
│   ├── shared/                       # Shared Module
│   │   ├── components/
│   │   │   ├── modal/
│   │   │   ├── pagination/
│   │   │   ├── file-upload/
│   │   │   └── status-badge/
│   │   ├── pipes/
│   │   │   ├── date.pipe.ts
│   │   │   └── status.pipe.ts
│   │   └── directives/
│   │       └── permission.directive.ts
│   │
│   ├── features/                     # Feature Modules (Lazy Loaded)
│   │   ├── auth/                     # /auth
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   └── forgot-password/
│   │   │
│   │   ├── dashboard/                # /dashboard
│   │   │   ├── components/
│   │   │   │   ├── stats-card/
│   │   │   │   ├── recent-screenings/
│   │   │   │   └── activity-chart/
│   │   │   └── dashboard.component.ts
│   │   │
│   │   ├── screening/                # /screenings
│   │   │   ├── screening-list/
│   │   │   ├── screening-detail/
│   │   │   ├── create-screening/
│   │   │   └── screening-workflow/
│   │   │
│   │   ├── documents/                # /documents
│   │   │   ├── document-upload/
│   │   │   ├── document-list/
│   │   │   └── document-preview/
│   │   │
│   │   ├── candidates/               # /candidates
│   │   │   ├── candidate-list/
│   │   │   └── candidate-detail/
│   │   │
│   │   ├── reports/                  # /reports
│   │   │   ├── compliance-report/
│   │   │   ├── sla-report/
│   │   │   └── export-report/
│   │   │
│   │   ├── admin/                    # /admin (Admin Only)
│   │   │   ├── user-management/
│   │   │   ├── tenant-settings/
│   │   │   ├── audit-logs/
│   │   │   └── system-config/
│   │   │
│   │   └── verifier/                 # /verifier (Verifier Only)
│   │       ├── verification-queue/
│   │       └── verification-result/
│   │
│   ├── layout/                       # Layout Components
│   │   ├── header/
│   │   ├── sidebar/
│   │   ├── footer/
│   │   └── main-layout.component.ts
│   │
│   └── app.routes.ts                 # Route Configuration
│
├── assets/
├── environments/
└── styles/
```

---

## 5. Data Model (Simplified)

```csharp
// User & Organization
User: Id, Email, PasswordHash, FirstName, LastName, Role, TenantId, IsActive, CreatedAt
Organization: Id, Name, SubscriptionPlan, Settings (JSON), CreatedAt

// Screening Core
ScreeningRequest: Id, CandidateId, OrganizationId, Status, Priority, 
                  DueDate, CreatedById, AssignedVerifierId, CompletedAt
ScreeningPackage: Id, Name, Description, DocumentTypes (List), SLAHours

Candidate: Id, Email, FirstName, LastName, DateOfBirth, Nationality,
           Address, Phone, ExternalId, CreatedAt

Document: Id, ScreeningRequestId, Type, FileName, FilePath, MimeType,
          FileSize, Status, OcrResult (JSON), VerificationResult,
          UploadedById, ExpiresAt, CreatedAt

// Verification Results
VerificationResult: Id, DocumentId, VerifierId, Status, ConfidenceScore,
                    Notes, Evidence (JSON), VerifiedAt
VerificationStep: Id, ScreeningRequestId, StepType, Status, Order

// GDPR & Audit
DataAccessRequest: Id, CandidateId, Type (Export/Delete), Status, 
                   RequestedAt, CompletedAt, CompletedBy
AuditLog: Id, UserId, Action, EntityType, EntityId, OldValue, NewValue,
          IpAddress, UserAgent, Timestamp

// Notifications
Notification: Id, UserId, Type, Title, Message, Read, CreatedAt
```

---

## 6. API Design (RESTful)

### 6.1 Base URL Structure

```
https://api.validata.local/v1/
```

### 6.2 Key Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| **Auth** |||
| POST | `/api/v1/auth/login` | User login |
| POST | `/api/v1/auth/register` | New user registration |
| POST | `/api/v1/auth/refresh` | Refresh JWT token |
| POST | `/api/v1/auth/forgot-password` | Password reset request |
| **Screenings** |||
| GET | `/api/v1/screenings` | List screenings (paginated) |
| POST | `/api/v1/screenings` | Create new screening |
| GET | `/api/v1/screenings/{id}` | Get screening details |
| PUT | `/api/v1/screenings/{id}` | Update screening |
| GET | `/api/v1/screenings/{id}/workflow` | Get workflow status |
| **Documents** |||
| POST | `/api/v1/documents/upload` | Upload document |
| GET | `/api/v1/documents` | List documents |
| GET | `/api/v1/documents/{id}` | Get document details |
| DELETE | `/api/v1/documents/{id}` | Delete document |
| **Candidates** |||
| GET | `/api/v1/candidates` | List candidates |
| POST | `/api/v1/candidates` | Create candidate |
| GET | `/api/v1/candidates/{id}` | Get candidate details |
| **Reports** |||
| GET | `/api/v1/reports/compliance` | Compliance report |
| GET | `/api/v1/reports/sla` | SLA report |
| GET | `/api/v1/reports/export?format=pdf` | Export report |
| **GDPR** |||
| POST | `/api/v1/gdpr/data-request` | Request data export |
| DELETE | `/api/v1/gdpr/data-request/{id}` | Request data deletion |
| **Admin** |||
| GET | `/api/v1/admin/audit-logs` | View audit logs |
| GET | `/api/v1/admin/users` | User management |

### 6.3 Response Models

```json
// Screening Response
{
  "id": "uuid",
  "candidateId": "uuid",
  "candidateName": "John Doe",
  "status": "InProgress",
  "priority": "Normal",
  "createdAt": "2025-02-10T10:00:00Z",
  "dueDate": "2025-02-17T10:00:00Z",
  "steps": [
    { "type": "IdentityCheck", "status": "Completed", "completedAt": "2025-02-11T14:00:00Z" },
    { "type": "CriminalRecord", "status": "InProgress", "assignedTo": "Verifier123" },
    { "type": "Education", "status": "Pending", "assignedTo": null }
  ]
}

// Document Response
{
  "id": "uuid",
  "type": "Passport",
  "fileName": "passport_scan.pdf",
  "status": "Verified",
  "confidenceScore": 0.95,
  "ocrResult": {
    "documentNumber": "AB1234567",
    "expiryDate": "2030-01-01",
    "nationality": "NL"
  },
  "uploadedAt": "2025-02-10T10:30:00Z"
}
```

---

## 7. Security & GDPR Compliance

### 7.1 Security Measures

| Layer | Implementation |
|-------|----------------|
| **Authentication** | JWT with short expiration (15min), refresh token rotation |
| **Authorization** | Role-based (RBAC) + Resource-based permissions |
| **Data Encryption** | AES-256 at rest, TLS 1.3 in transit |
| **Secrets** | Azure Key Vault / HashiCorp Vault |
| **Input Validation** | FluentValidation, input sanitization |
| **API Security** | Rate limiting, CORS, API keys for external integrations |
| **Password Policy** | Min 12 chars, complexity, bcrypt (cost=12) |

### 7.2 GDPR Requirements

| Requirement | Implementation |
|-------------|---------------|
| **Data Minimization** | Only collect required fields, document retention policies |
| **Purpose Limitation** | Clear consent purposes, audit trail of data usage |
| **Right to Access** | Self-service portal to export personal data (PDF/JSON) |
| **Right to Erasure** | Automated deletion workflow, cascade handling |
| **Right to Rectification** | Edit profile data, document updates |
| **Data Portability** | Export in machine-readable format (JSON) |
| **Consent Management** | Granular consent tracking, withdrawal capability |
| **DPO Contact** | Configurable DPO contact in system |
| **Data Processing Records** | Automated logging of all data access |
| **Data Protection by Design** | Privacy impact assessments, pseudonymization |

### 7.3 Audit Trail

All actions must be logged:
- User login/logout
- Document upload/download
- Screening status changes
- Data exports
- Permission changes
- GDPR requests

---

## 8. External Integrations

| Provider | Purpose | Type |
|----------|---------|------|
| **Identity Verification** | iDenfy, Jumio, Onfido | Paid API |
| **OCR** | Azure Document Intelligence, Tesseract | API/Self-hosted |
| **Criminal Records** | National police databases (per country) | Government API |
| **Education** | National databases, university direct contact | API/Manual |
| **Employment** | LinkedIn API, direct employer contact | API/Manual |
| **Email** | SendGrid, Azure Communication Services | Transactional |
| **Storage** | Azure Blob Storage (EU region) | Object storage |
| **Monitoring** | Application Insights, Serilog | Logging |
