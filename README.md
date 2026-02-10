# Validata - Employment Documentation Validation Platform

An end-to-end employment screening platform built with .NET 10 and Angular 19. Features include candidate screening, document verification, OCR processing, GDPR compliance, and comprehensive reporting dashboards.

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Web API
- **Frontend:** Angular 19, TypeScript, SCSS
- **Database:** SQL Server (Azure SQL)
- **Authentication:** JWT with refresh tokens
- **Infrastructure:** Azure App Service, Azure Blob Storage
- **CI/CD:** GitHub Actions, Docker

## Project Structure

```
validata/
├── src/
│   ├── Validata.Api/              # REST API project
│   │   ├── Controllers/           # API controllers (Auth, Screenings, Documents, GDPR, Reports, Audit)
│   │   ├── HealthChecks/          # Health check endpoints
│   │   └── Program.cs
│   │
│   ├── Validata.Core/             # Domain layer
│   │   ├── Entities/             # Domain entities (User, Candidate, ScreeningRequest, Document, etc.)
│   │   ├── Enums/                # Enumerations (UserRole, ScreeningStatus, DocumentType, etc.)
│   │   └── Interfaces/           # Repository & service interfaces
│   │
│   ├── Validata.Application/      # Application layer
│   │   └── DTOs/                 # Data transfer objects for all entities
│   │
│   ├── Validata.Infrastructure/   # Infrastructure layer
│   │   ├── Data/                 # DbContext & repositories
│   │   ├── Identity/             # Auth services (JWT, Password hashing)
│   │   └── Services/            # Business services (OCR, Workflow, Reporting, GDPR)
│   │
│   ├── Validata.Integration/     # External integrations
│   │   └── Placeholder for third-party APIs
│   │
│   ├── Validata.Worker/          # Background jobs
│   │
│   └── Validata.Web/             # Angular 19 frontend
│       ├── src/
│       │   ├── app/
│       │   │   ├── core/         # Core services, guards, interceptors
│       │   │   ├── features/     # Feature modules (auth, dashboard, documents, gdpr, reports, audit)
│       │   │   └── shared/       # Shared components
│       │   └── environments/
│       └── angular.json
│
├── tests/
│   └── Validata.Tests/           # Unit tests (xUnit)
│
├── .github/
│   └── workflows/
│       └── ci-cd.yml             # CI/CD pipeline
│
├── TECHNICAL_SPECIFICATIONS.md
├── DEVELOPMENT_PLAN.md
├── API_SPECIFICATION.md
├── INFRASTRUCTURE.md
└── README.md
```

## Features

### Phase 1: Foundation
- JWT authentication with refresh tokens
- Role-based access control (Admin, HR Manager, HR Staff, Verifier, Compliance Officer)
- Candidate and screening request management
- Document upload and storage

### Phase 2: Document Processing
- Azure Blob Storage integration
- OCR processing with Azure Document Intelligence
- Document validation and verification

### Phase 3: Verification Workflows
- Screening workflow engine with state machine
- SLA monitoring and notifications
- Multiple verification types (Identity, Criminal Record, Education, Employment, Tax, Reference)

### Phase 4: GDPR & Compliance
- GDPR self-service portal (data export, deletion, rectification, restrict processing)
- Consent management
- Comprehensive audit logging

### Phase 5: Reporting & Dashboard
- Real-time KPI dashboard
- Screening trends analysis
- SLA compliance reports
- CSV export functionality

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- SQL Server (or Azure SQL)
- Azure Storage Account (for blob storage)

### Backend Setup

```bash
# Restore dependencies
dotnet restore src/Validata.sln

# Build
dotnet build src/Validata.sln

# Run migrations (when database is available)
dotnet ef migrations add InitialCreate --project src/Validata.Infrastructure

# Run API
cd src/Validata.Api
dotnet run
```

### Frontend Setup

```bash
cd src/Validata.Web

# Install dependencies
npm install

# Run development server
ng serve
```

## Environment Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ValidataDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "your-secure-secret-key-minimum-32-characters",
    "Issuer": "Validata",
    "Audience": "ValidataUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
    "ContainerName": "documents"
  },
  "DocumentIntelligence": {
    "Endpoint": "https://your-resource.cognitiveservices.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

### Frontend (environment.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1'
};
```

## API Endpoints

### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/refresh` - Refresh access token

### Screenings
- `GET /api/v1/screenings` - List screenings (with filters)
- `POST /api/v1/screenings` - Create screening
- `GET /api/v1/screenings/{id}` - Get screening details
- `PUT /api/v1/screenings/{id}/status` - Update status
- `POST /api/v1/screenings/{id}/steps` - Add verification step

### Documents
- `GET /api/v1/documents` - List documents
- `POST /api/v1/documents/upload` - Upload document
- `GET /api/v1/documents/{id}` - Get document details
- `POST /api/v1/documents/{id}/verify` - Verify document

### GDPR
- `POST /api/v1/gdpr/requests` - Create GDPR request
- `GET /api/v1/gdpr/requests/{id}` - Get request status
- `GET /api/v1/gdpr/export/{candidateId}` - Export candidate data
- `DELETE /api/v1/gdpr/candidates/{candidateId}` - Delete candidate data

### Reports
- `GET /api/v1/reports/kpi` - Get KPI summary
- `GET /api/v1/reports/dashboard` - Get dashboard stats
- `GET /api/v1/reports/trends` - Get screening trends
- `GET /api/v1/reports/sla` - Get SLA compliance report
- `GET /api/v1/reports/generate` - Generate report export

### Audit
- `GET /api/v1/audit` - Get audit logs
- `GET /api/v1/audit/user/{userId}` - Get user audit logs

## Health Checks

- `GET /health` - Overall health check
- `GET /health/ready` - Readiness check
- `GET /health/live` - Liveness check

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Deployment

### Azure App Service

1. Create Azure App Service (.NET 10)
2. Configure application settings
3. Set up Azure SQL Database
4. Configure Azure Storage account
5. Deploy via GitHub Actions

### Docker

```bash
# Build API image
docker build -t validata-api -f src/Validata.Api/Dockerfile .

# Build Frontend image
docker build -t validata-web -f src/Validata.Web/Dockerfile .

# Run containers
docker-compose up -d
```

## Security Considerations

- Passwords hashed using PBKDF2 with SHA256 (100,000 iterations)
- JWT tokens with short expiry (15 minutes)
- Refresh tokens for session management
- Input sanitization on all user inputs
- HTTPS enforced in production
- Rate limiting recommended for production

## License

MIT
