namespace Validata.Core.Enums;

public enum UserRole
{
    Admin = 1,
    HRManager = 2,
    HRStaff = 3,
    Candidate = 4,
    Verifier = 5,
    ComplianceOfficer = 6
}

public enum ScreeningStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    OnHold = 5
}

public enum DocumentType
{
    Passport = 1,
    IDCard = 2,
    DriversLicense = 3,
    Diploma = 4,
    Certificate = 5,
    WorkHistory = 6,
    TaxDocument = 7,
    CriminalRecord = 8,
    ReferenceLetter = 9,
    Other = 10
}

public enum DocumentStatus
{
    Uploaded = 1,
    Processing = 2,
    PendingReview = 3,
    Verified = 4,
    Rejected = 5,
    Expired = 6
}

public enum VerificationType
{
    IdentityCheck = 1,
    CriminalRecord = 2,
    Education = 3,
    Employment = 4,
    Tax = 5,
    Reference = 6
}

public enum VerificationStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum Priority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum GDPRRequestType
{
    Export = 1,
    Delete = 2,
    Rectify = 3,
    Restrict = 4
}

public enum GDPRRequestStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Rejected = 4
}
