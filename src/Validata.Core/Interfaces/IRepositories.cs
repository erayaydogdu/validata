using Validata.Core.Entities;

namespace Validata.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(int role);
    Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId);
}

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByNameAsync(string name);
}

public interface ICandidateRepository : IRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email);
    Task<IEnumerable<Candidate>> GetByOrganizationIdAsync(Guid organizationId);
}

public interface IScreeningRequestRepository : IRepository<ScreeningRequest>
{
    Task<IEnumerable<ScreeningRequest>> GetByStatusAsync(int status);
    Task<IEnumerable<ScreeningRequest>> GetByOrganizationIdAsync(Guid organizationId);
    Task<IEnumerable<ScreeningRequest>> GetByCandidateIdAsync(Guid candidateId);
    Task<IEnumerable<ScreeningRequest>> GetByVerifierIdAsync(Guid verifierId);
}

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByScreeningRequestIdAsync(Guid screeningRequestId);
    Task<IEnumerable<Document>> GetByCandidateIdAsync(Guid candidateId);
    Task<IEnumerable<Document>> GetByTypeAsync(int type);
}

public interface IVerificationStepRepository : IRepository<VerificationStep>
{
    Task<IEnumerable<VerificationStep>> GetByScreeningRequestIdAsync(Guid screeningRequestId);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
}

public interface IGDPRRequestRepository : IRepository<GDPRRequest>
{
    Task<IEnumerable<GDPRRequest>> GetByCandidateIdAsync(Guid candidateId);
    Task<IEnumerable<GDPRRequest>> GetByStatusAsync(int status);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
}
