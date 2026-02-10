namespace Validata.Core.Interfaces;

public interface IScreeningService
{
    Task<object> CreateScreeningAsync(object request);
    Task<object?> GetScreeningByIdAsync(Guid id);
    Task<IEnumerable<object>> GetScreeningsAsync(object query);
    Task<object> UpdateScreeningStatusAsync(Guid id, int status);
    Task<bool> AssignVerifierAsync(Guid screeningId, Guid verifierId);
    Task<bool> CancelScreeningAsync(Guid id);
}

public interface IDocumentService
{
    Task<object> UploadDocumentAsync(object request);
    Task<object?> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<object>> GetDocumentsAsync(object query);
    Task<bool> DeleteDocumentAsync(Guid id);
    Task<object> ProcessDocumentAsync(Guid id);
}

public interface ICandidateService
{
    Task<object> CreateCandidateAsync(object request);
    Task<object?> GetCandidateByIdAsync(Guid id);
    Task<object?> GetCandidateByEmailAsync(string email);
    Task<IEnumerable<object>> GetCandidatesAsync(object query);
    Task<object> UpdateCandidateAsync(Guid id, object request);
}

public interface IAuthService
{
    Task<object> RegisterAsync(object request);
    Task<object> LoginAsync(object request);
    Task<object> RefreshTokenAsync(string refreshToken);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(object request);
}

public interface IAuditService
{
    Task LogActionAsync(object request);
    Task<IEnumerable<object>> GetAuditLogsAsync(object query);
}

public interface IGdprService
{
    Task<object> CreateDataRequestAsync(object request);
    Task<object?> GetRequestByIdAsync(Guid id);
    Task<object> CompleteRequestAsync(Guid id, string result);
    Task<string> ExportCandidateDataAsync(Guid candidateId);
    Task<bool> DeleteCandidateDataAsync(Guid candidateId);
}

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream?> DownloadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> GetFileUrlAsync(string filePath);
    Task<string> UploadDocumentAsync(Guid documentId, Stream fileStream, string fileName, string contentType);
}

public interface IVerificationService
{
    Task<object> StartVerificationAsync(Guid screeningId, int verificationType);
    Task<object> CompleteVerificationAsync(Guid stepId, string result);
    Task<IEnumerable<object>> GetVerificationStepsAsync(Guid screeningId);
}
