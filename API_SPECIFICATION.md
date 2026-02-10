# API Specification: Employment Documentation Validation Platform

## Base URL

```
Development: https://api-dev.validata.local/v1/
Staging:      https://api-staging.validata.local/v1/
Production:   https://api.validata.local/v1/
```

## Authentication

All API requests require JWT authentication:
```
Authorization: Bearer <access_token>
```

---

## Endpoints

### Authentication

#### POST /api/v1/auth/login
Request:
```json
{
  "email": "user@example.com",
  "password": "securePassword123!"
}
```
Response (200):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "def50200f8c2e...",
  "expiresIn": 900,
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "HRManager"
  }
}
```

#### POST /api/v1/auth/refresh
Request:
```json
{
  "refreshToken": "def50200f8c2e..."
}
```
Response (200):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "def50200a7b3d...",
  "expiresIn": 900
}
```

#### POST /api/v1/auth/register
Request:
```json
{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "organizationName": "Acme Corp"
}
```

---

### Screenings

#### GET /api/v1/screenings
Query Parameters:
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `status` (string)
- `candidateId` (uuid)
- `fromDate` (date)
- `toDate` (date)

Response (200):
```json
{
  "data": [
    {
      "id": "uuid",
      "candidateId": "uuid",
      "candidateName": "John Doe",
      "status": "InProgress",
      "priority": "Normal",
      "createdAt": "2025-02-10T10:00:00Z",
      "dueDate": "2025-02-17T10:00:00Z",
      "progress": 45
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

#### POST /api/v1/screenings
Request:
```json
{
  "candidateId": "uuid",
  "packageId": "uuid",
  "priority": "Normal",
  "dueDate": "2025-02-17T10:00:00Z",
  "notes": "Urgent hire, expedite please"
}
```
Response (201):
```json
{
  "id": "uuid",
  "status": "Pending",
  "createdAt": "2025-02-10T10:00:00Z"
}
```

#### GET /api/v1/screenings/{id}
Response (200):
```json
{
  "id": "uuid",
  "candidateId": "uuid",
  "candidateName": "John Doe",
  "status": "InProgress",
  "priority": "Normal",
  "createdAt": "2025-02-10T10:00:00Z",
  "dueDate": "2025-02-17T10:00:00Z",
  "completedAt": null,
  "steps": [
    {
      "id": "uuid",
      "type": "IdentityCheck",
      "name": "Identity Verification",
      "status": "Completed",
      "completedAt": "2025-02-11T14:00:00Z",
      "result": "Verified"
    },
    {
      "id": "uuid",
      "type": "CriminalRecord",
      "name": "Criminal Record Check",
      "status": "InProgress",
      "assignedTo": "verifier@example.com",
      "startedAt": "2025-02-12T09:00:00Z"
    },
    {
      "id": "uuid",
      "type": "Education",
      "name": "Education Verification",
      "status": "Pending",
      "assignedTo": null
    }
  ]
}
```

---

### Documents

#### POST /api/v1/documents/upload
Content-Type: multipart/form-data

Form Fields:
- `file` (binary)
- `type` (string: Passport, IDCard, Diploma, WorkHistory, TaxDocument, Other)
- `screeningId` (uuid, optional)
- `candidateId` (uuid, required if no screeningId)

Response (201):
```json
{
  "id": "uuid",
  "fileName": "passport_scan.pdf",
  "type": "Passport",
  "status": "Processing",
  "uploadedAt": "2025-02-10T10:30:00Z"
}
```

#### GET /api/v1/documents/{id}
Response (200):
```json
{
  "id": "uuid",
  "type": "Passport",
  "fileName": "passport_scan.pdf",
  "fileSize": 245760,
  "mimeType": "application/pdf",
  "status": "Verified",
  "confidenceScore": 0.95,
  "ocrResult": {
    "documentType": "Passport",
    "documentNumber": "AB1234567",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1985-03-15",
    "expiryDate": "2030-01-01",
    "nationality": "NL"
  },
  "verificationResult": {
    "status": "Verified",
    "verifiedBy": "verifier@example.com",
    "verifiedAt": "2025-02-11T14:00:00Z",
    "notes": "Document verified successfully"
  },
  "uploadedAt": "2025-02-10T10:30:00Z"
}
```

#### GET /api/v1/documents/{id}/download
Response: File download (200)

---

### Candidates

#### GET /api/v1/candidates
Response (200):
```json
{
  "data": [
    {
      "id": "uuid",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "dateOfBirth": "1985-03-15",
      "nationality": "NL",
      "createdAt": "2025-02-10T10:00:00Z"
    }
  ]
}
```

#### POST /api/v1/candidates
Request:
```json
{
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1985-03-15",
  "nationality": "NL",
  "address": {
    "street": "Main Street 123",
    "city": "Amsterdam",
    "postalCode": "1011AB",
    "country": "NL"
  },
  "phone": "+31 6 12345678"
}
```

---

### Reports

#### GET /api/v1/reports/compliance
Query Parameters:
- `fromDate` (date, required)
- `toDate` (date, required)
- `format` (json | pdf | excel, default: json)

Response (200):
```json
{
  "period": {
    "from": "2025-01-01",
    "to": "2025-01-31"
  },
  "summary": {
    "totalScreenings": 150,
    "completedScreenings": 120,
    "inProgressScreenings": 25,
    "cancelledScreenings": 5,
    "averageCompletionDays": 4.2
  },
  "byType": {
    "IdentityCheck": { "completed": 145, "pending": 5 },
    "CriminalRecord": { "completed": 120, "pending": 30 },
    "Education": { "completed": 100, "pending": 50 }
  }
}
```

#### GET /api/v1/reports/sla
Response (200):
```json
{
  "period": "2025-01",
  "metrics": [
    {
      "screeningType": "Standard",
      "targetSlaHours": 168,
      "averageCompletionHours": 142,
      "complianceRate": 0.95
    },
    {
      "screeningType": "Express",
      "targetSlaHours": 72,
      "averageCompletionHours": 68,
      "complianceRate": 0.88
    }
  ]
}
```

---

### GDPR

#### POST /api/v1/gdpr/data-request
Request:
```json
{
  "type": "Export",
  "candidateId": "uuid",
  "reason": "Requesting my personal data"
}
```
Response (202):
```json
{
  "requestId": "uuid",
  "status": "Processing",
  "estimatedCompletionDays": 30,
  "willReceiveEmail": true
}
```

#### GET /api/v1/gdpr/data-request/{id}
Response (200):
```json
{
  "id": "uuid",
  "type": "Export",
  "status": "Completed",
  "requestedAt": "2025-02-10T10:00:00Z",
  "completedAt": "2025-02-12T14:00:00Z",
  "downloadUrl": "/api/v1/gdpr/data-request/uuid/download"
}
```

---

### Admin

#### GET /api/v1/admin/audit-logs
Query Parameters:
- `userId` (uuid)
- `action` (string)
- `fromDate` (date)
- `toDate` (date)
- `page` (int)
- `pageSize` (int)

Response (200):
```json
{
  "data": [
    {
      "id": "uuid",
      "userId": "uuid",
      "userEmail": "admin@example.com",
      "action": "Document.Delete",
      "entityType": "Document",
      "entityId": "uuid",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0...",
      "timestamp": "2025-02-10T15:30:00Z"
    }
  ]
}
```

#### GET /api/v1/admin/users
Response (200):
```json
{
  "data": [
    {
      "id": "uuid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "HRManager",
      "organizationName": "Acme Corp",
      "isActive": true,
      "lastLoginAt": "2025-02-10T09:00:00Z"
    }
  ]
}
```

---

## HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 422 | Validation Error |
| 429 | Rate Limited |
| 500 | Internal Server Error |

---

## Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "email",
        "message": "The email field is required"
      },
      {
        "field": "password",
        "message": "Password must be at least 8 characters"
      }
    ]
  },
  "traceId": "uuid"
}
```

---

## Rate Limiting

| Tier | Requests/Minute |
|------|----------------|
| Free | 60 |
| Standard | 300 |
| Enterprise | 1000 |

Rate limit headers included in response:
```
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 299
X-RateLimit-Reset: 1739200000
```
