# Infrastructure Specification

## Recommended Stack

| Component | Azure | Alternative |
|-----------|-------|-------------|
| Compute | Azure App Service (Containers) | AWS ECS, Self-hosted Kubernetes |
| Database | Azure SQL (EU) | PostgreSQL on RDS |
| Blob Storage | Azure Blob Storage (EU) | AWS S3 (EU) |
| Cache | Azure Cache for Redis | ElastiCache |
| Message Queue | Azure Service Bus | RabbitMQ |
| Search | Azure Cognitive Search | Elasticsearch |
| Monitoring | Azure Application Insights | Datadog |
| CI/CD | Azure DevOps | GitHub Actions |
| DNS | Azure DNS | Cloudflare |
| CDN | Azure CDN | Cloudflare |

---

## Environment Strategy

```
┌─────────────────────────────────────────────────────────┐
│  Development (DEV)    - Developer testing               │
│  Testing (TEST)      - QA/Integration testing          │
│  Staging (STAGING)   - Production mirror, UAT         │
│  Production (PROD)   - Live environment                │
└─────────────────────────────────────────────────────────┘
```

### DEV Environment
- Single instance
- Shared database
- Minimal monitoring
- Automatic deployments on push

### TEST Environment
- Isolated resources
- Test data
- Manual deployments
- Integration testing

### STAGING Environment
- Production-like sizing
- Production data (anonymized)
- Manual approval deployments
- UAT testing

### PROD Environment
- Multi-AZ deployment
- Full monitoring
- Blue/green deployments
- Auto-scaling

---

## Database Schema

### Core Tables

```sql
CREATE TABLE Organizations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    SubscriptionPlan NVARCHAR(50),
    Settings NVARCHAR(MAX),
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER FOREIGN KEY,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

CREATE TABLE Candidates (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    DateOfBirth DATE,
    Nationality NVARCHAR(10),
    Address NVARCHAR(MAX),
    Phone NVARCHAR(50),
    ExternalId NVARCHAR(100),
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

CREATE TABLE ScreeningRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CandidateId UNIQUEIDENTIFIER FOREIGN KEY,
    OrganizationId UNIQUEIDENTIFIER FOREIGN KEY,
    Status NVARCHAR(50) NOT NULL,
    Priority NVARCHAR(50),
    DueDate DATETIME2,
    CreatedById UNIQUEIDENTIFIER FOREIGN KEY,
    AssignedVerifierId UNIQUEIDENTIFIER FOREIGN KEY,
    CompletedAt DATETIME2,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ScreeningRequestId UNIQUEIDENTIFIER FOREIGN KEY,
    CandidateId UNIQUEIDENTIFIER FOREIGN KEY,
    Type NVARCHAR(50) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    MimeType NVARCHAR(100),
    FileSize BIGINT,
    Status NVARCHAR(50),
    OcrResult NVARCHAR(MAX),
    VerificationResult NVARCHAR(MAX),
    UploadedById UNIQUEIDENTIFIER FOREIGN KEY,
    ExpiresAt DATETIME2,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

CREATE TABLE VerificationSteps (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ScreeningRequestId UNIQUEIDENTIFIER FOREIGN KEY,
    StepType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50),
    AssignedTo UNIQUEIDENTIFIER FOREIGN KEY,
    StartedAt DATETIME2,
    CompletedAt DATETIME2,
    Result NVARCHAR(MAX)
);

CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId UNIQUEIDENTIFIER,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    Timestamp DATETIME2 NOT NULL
);

CREATE TABLE DataAccessRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CandidateId UNIQUEIDENTIFIER FOREIGN KEY,
    Type NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    RequestedAt DATETIME2 NOT NULL,
    CompletedAt DATETIME2,
    CompletedBy UNIQUEIDENTIFIER FOREIGN KEY
);

CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER FOREIGN KEY,
    Type NVARCHAR(50) NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL
);
```

---

## CI/CD Pipeline (GitHub Actions)

```yaml
name: Validata CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  preflight:
    runs-on: ubuntu-latest
    outputs:
      deploy: ${{ steps.check.outputs.deploy }}
    steps:
      - id: check
        env:
          REGISTRY: ${{ secrets.REGISTRY_LOGIN_SERVER }}
        run: |
          if [ -n "$REGISTRY" ]; then
            echo "deploy=true" >> "$GITHUB_OUTPUT"
            echo "Registry configured - docker/deploy jobs will run."
          else
            echo "deploy=false" >> "$GITHUB_OUTPUT"
            echo "Registry secrets not configured - skipping docker/deploy jobs."
          fi

  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v5
      
      - name: Setup .NET 10
        uses: actions/setup-dotnet@v5
        with:
          dotnet-version: 10.0.x
      
      - name: Setup Node.js
        uses: actions/setup-node@v5
        with:
          node-version: '24'
      
      - name: Restore Dependencies (.NET)
        run: dotnet restore Validata.slnx
      
      - name: Build (.NET)
        run: dotnet build Validata.slnx --no-restore
      
      - name: Run Tests (.NET)
        run: dotnet test Validata.slnx --no-build --verbosity minimal --collect:"Xplat Code Coverage"
      
      - name: Install Angular
        run: npm install --prefix src/Validata.Web
      
      - name: Build Angular
        run: npm run build --prefix src/Validata.Web
      
      - name: Upload Coverage
        uses: codecov/codecov-action@v6
        with:
          files: ./coverage/*/coverage.cobertura.xml
          fail_ci_if_error: false

  docker:
    needs: [test, preflight]
    runs-on: ubuntu-latest
    if: needs.preflight.outputs.deploy == 'true' && (github.ref == 'refs/heads/main' || github.ref == 'refs/heads/develop')
    steps:
      - uses: actions/checkout@v5
      
      - name: Login to Container Registry
        uses: docker/login-action@v4
        with:
          registry: ${{ secrets.REGISTRY_LOGIN_SERVER }}
          username: ${{ secrets.REGISTRY_USERNAME }}
          password: ${{ secrets.REGISTRY_PASSWORD }}
      
      - name: Build and Push API
        run: |
          docker build -t ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-api:${{ github.sha }} -f src/Validata.Api/Dockerfile .
          docker push ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-api:${{ github.sha }}
      
      - name: Build and Push Frontend
        run: |
          docker build -t ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-web:${{ github.sha }} -f src/Validata.Web/Dockerfile .
          docker push ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-web:${{ github.sha }}

  deploy-dev:
    needs: [docker, preflight]
    runs-on: ubuntu-latest
    if: needs.preflight.outputs.deploy == 'true' && github.ref == 'refs/heads/develop'
    environment: Development
    steps:
      - name: Deploy to Development
        uses: azure/webapps-deploy@v3
        with:
          app-name: validata-api-dev
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_DEV }}
          images: ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-api:${{ github.sha }}

  deploy-staging:
    needs: [docker, preflight]
    runs-on: ubuntu-latest
    if: needs.preflight.outputs.deploy == 'true' && github.ref == 'refs/heads/main'
    environment: Staging
    steps:
      - name: Deploy to Staging
        uses: azure/webapps-deploy@v3
        with:
          app-name: validata-api-staging
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_STAGING }}
          images: ${{ secrets.REGISTRY_LOGIN_SERVER }}/validata-api:${{ github.sha }}
```

> The workflow above mirrors `.github/workflows/ci-cd.yml`, which is the source of truth.

---

## Docker Configuration

### Backend Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Validata.Core/Validata.Core.csproj src/Validata.Core/
COPY src/Validata.Application/Validata.Application.csproj src/Validata.Application/
COPY src/Validata.Infrastructure/Validata.Infrastructure.csproj src/Validata.Infrastructure/
COPY src/Validata.Integration/Validata.Integration.csproj src/Validata.Integration/
COPY src/Validata.Api/Validata.Api.csproj src/Validata.Api/
RUN dotnet restore src/Validata.Api/Validata.Api.csproj

COPY src/Validata.Core/ src/Validata.Core/
COPY src/Validata.Application/ src/Validata.Application/
COPY src/Validata.Infrastructure/ src/Validata.Infrastructure/
COPY src/Validata.Integration/ src/Validata.Integration/
COPY src/Validata.Api/ src/Validata.Api/

RUN dotnet publish src/Validata.Api/Validata.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Validata.Api.dll"]
```

### Frontend Dockerfile
```dockerfile
FROM node:22-alpine AS build
WORKDIR /app

COPY src/Validata.Web/package.json src/Validata.Web/package-lock.json* ./
RUN npm ci

COPY src/Validata.Web/ .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist/validata-web/browser /usr/share/nginx/html
COPY src/Validata.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

---

## Monitoring & Observability

### Application Insights Configuration
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddAzureBlobStorage(storageConnectionString)
    .AddRedis(redisConnectionString);
```

### Logging (Serilog)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        TelemetryClient,
        TelemetryConverter.Traces)
    .CreateLogger();
```

---

## Security Configuration

### Azure Key Vault
```
Secrets stored:
- Database connection strings
- JWT secret keys
- API keys for external services
- Storage account keys
```

### Network Security
```
- Private endpoints for database
- VNet integration for App Service
- WAF with OWASP rules
- DDoS protection
```

### SSL/TLS
```
- TLS 1.3 minimum
- Certificate management via Key Vault
- HSTS headers enabled
```

---

## Backup & Disaster Recovery

### Backup Strategy
| Resource | Frequency | Retention |
|----------|-----------|-----------|
| Database | Hourly differential, Daily full | 30 days |
| Blob Storage | Daily replication to secondary region | 90 days |
| Application Config | Version controlled in Git | Indefinite |

### Recovery Objectives
- **RTO (Recovery Time Objective):** 4 hours
- **RPO (Recovery Point Objective):** 1 hour
