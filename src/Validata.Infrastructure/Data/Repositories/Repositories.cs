using Microsoft.EntityFrameworkCore;
using Validata.Core.Entities;
using Validata.Core.Interfaces;

namespace Validata.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id) => await _context.Users.FindAsync(id);

    public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();

    public async Task AddAsync(User entity) => await _context.Users.AddAsync(entity);

    public void Update(User entity) => _context.Users.Update(entity);

    public void Delete(User entity) => _context.Users.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetByRoleAsync(int role) =>
        await _context.Users.Where(u => u.Role == role).ToListAsync();

    public async Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId) =>
        await _context.Users.Where(u => u.OrganizationId == organizationId).ToListAsync();
}

public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _context;

    public OrganizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id) => await _context.Organizations.FindAsync(id);

    public async Task<IEnumerable<Organization>> GetAllAsync() => await _context.Organizations.ToListAsync();

    public async Task AddAsync(Organization entity) => await _context.Organizations.AddAsync(entity);

    public void Update(Organization entity) => _context.Organizations.Update(entity);

    public void Delete(Organization entity) => _context.Organizations.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<Organization?> GetByNameAsync(string name) =>
        await _context.Organizations.FirstOrDefaultAsync(o => o.Name == name);
}

public class CandidateRepository : ICandidateRepository
{
    private readonly AppDbContext _context;

    public CandidateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Candidate?> GetByIdAsync(Guid id) => await _context.Candidates.FindAsync(id);

    public async Task<IEnumerable<Candidate>> GetAllAsync() => await _context.Candidates.ToListAsync();

    public async Task AddAsync(Candidate entity) => await _context.Candidates.AddAsync(entity);

    public void Update(Candidate entity) => _context.Candidates.Update(entity);

    public void Delete(Candidate entity) => _context.Candidates.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<Candidate?> GetByEmailAsync(string email) =>
        await _context.Candidates.FirstOrDefaultAsync(c => c.Email == email);

    public async Task<IEnumerable<Candidate>> GetByOrganizationIdAsync(Guid organizationId) =>
        await _context.Candidates
            .Where(c => c.ScreeningRequests.Any(s => s.OrganizationId == organizationId))
            .ToListAsync();
}

public class ScreeningRequestRepository : IScreeningRequestRepository
{
    private readonly AppDbContext _context;

    public ScreeningRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ScreeningRequest?> GetByIdAsync(Guid id) =>
        await _context.ScreeningRequests
            .Include(s => s.Candidate)
            .Include(s => s.VerificationSteps)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<ScreeningRequest>> GetAllAsync() =>
        await _context.ScreeningRequests
            .Include(s => s.Candidate)
            .ToListAsync();

    public async Task AddAsync(ScreeningRequest entity) => await _context.ScreeningRequests.AddAsync(entity);

    public void Update(ScreeningRequest entity) => _context.ScreeningRequests.Update(entity);

    public void Delete(ScreeningRequest entity) => _context.ScreeningRequests.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<ScreeningRequest>> GetByStatusAsync(int status) =>
        await _context.ScreeningRequests
            .Include(s => s.Candidate)
            .Where(s => s.Status == status)
            .ToListAsync();

    public async Task<IEnumerable<ScreeningRequest>> GetByOrganizationIdAsync(Guid organizationId) =>
        await _context.ScreeningRequests
            .Include(s => s.Candidate)
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync();

    public async Task<IEnumerable<ScreeningRequest>> GetByCandidateIdAsync(Guid candidateId) =>
        await _context.ScreeningRequests
            .Include(s => s.Organization)
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();

    public async Task<IEnumerable<ScreeningRequest>> GetByVerifierIdAsync(Guid verifierId) =>
        await _context.ScreeningRequests
            .Include(s => s.Candidate)
            .Where(s => s.AssignedVerifierId == verifierId)
            .ToListAsync();
}

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id) =>
        await _context.Documents.FindAsync(id);

    public async Task<IEnumerable<Document>> GetAllAsync() => await _context.Documents.ToListAsync();

    public async Task AddAsync(Document entity) => await _context.Documents.AddAsync(entity);

    public void Update(Document entity) => _context.Documents.Update(entity);

    public void Delete(Document entity) => _context.Documents.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Document>> GetByScreeningRequestIdAsync(Guid screeningRequestId) =>
        await _context.Documents
            .Where(d => d.ScreeningRequestId == screeningRequestId)
            .ToListAsync();

    public async Task<IEnumerable<Document>> GetByCandidateIdAsync(Guid candidateId) =>
        await _context.Documents
            .Where(d => d.CandidateId == candidateId)
            .ToListAsync();

    public async Task<IEnumerable<Document>> GetByTypeAsync(int type) =>
        await _context.Documents
            .Where(d => d.Type == type)
            .ToListAsync();
}

public class VerificationStepRepository : IVerificationStepRepository
{
    private readonly AppDbContext _context;

    public VerificationStepRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationStep?> GetByIdAsync(Guid id) =>
        await _context.VerificationSteps.FindAsync(id);

    public async Task<IEnumerable<VerificationStep>> GetAllAsync() =>
        await _context.VerificationSteps.ToListAsync();

    public async Task AddAsync(VerificationStep entity) =>
        await _context.VerificationSteps.AddAsync(entity);

    public void Update(VerificationStep entity) => _context.VerificationSteps.Update(entity);

    public void Delete(VerificationStep entity) => _context.VerificationSteps.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<VerificationStep>> GetByScreeningRequestIdAsync(Guid screeningRequestId) =>
        await _context.VerificationSteps
            .Where(v => v.ScreeningRequestId == screeningRequestId)
            .OrderBy(v => v.Order)
            .ToListAsync();
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id) =>
        await _context.AuditLogs.FindAsync(id);

    public async Task<IEnumerable<AuditLog>> GetAllAsync() =>
        await _context.AuditLogs.OrderByDescending(a => a.Timestamp).ToListAsync();

    public async Task AddAsync(AuditLog entity) =>
        await _context.AuditLogs.AddAsync(entity);

    public void Update(AuditLog entity) => _context.AuditLogs.Update(entity);

    public void Delete(AuditLog entity) => _context.AuditLogs.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId) =>
        await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId) =>
        await _context.AuditLogs
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        await _context.AuditLogs
            .Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
}

public class GDPRRequestRepository : IGDPRRequestRepository
{
    private readonly AppDbContext _context;

    public GDPRRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GDPRRequest?> GetByIdAsync(Guid id) =>
        await _context.GDPRRequests.FindAsync(id);

    public async Task<IEnumerable<GDPRRequest>> GetAllAsync() =>
        await _context.GDPRRequests.ToListAsync();

    public async Task AddAsync(GDPRRequest entity) =>
        await _context.GDPRRequests.AddAsync(entity);

    public void Update(GDPRRequest entity) => _context.GDPRRequests.Update(entity);

    public void Delete(GDPRRequest entity) => _context.GDPRRequests.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<GDPRRequest>> GetByCandidateIdAsync(Guid candidateId) =>
        await _context.GDPRRequests
            .Where(g => g.CandidateId == candidateId)
            .ToListAsync();

    public async Task<IEnumerable<GDPRRequest>> GetByStatusAsync(int status) =>
        await _context.GDPRRequests
            .Where(g => g.Status == status)
            .ToListAsync();
}

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id) =>
        await _context.Notifications.FindAsync(id);

    public async Task<IEnumerable<Notification>> GetAllAsync() =>
        await _context.Notifications.ToListAsync();

    public async Task AddAsync(Notification entity) =>
        await _context.Notifications.AddAsync(entity);

    public void Update(Notification entity) => _context.Notifications.Update(entity);

    public void Delete(Notification entity) => _context.Notifications.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId) =>
        await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId) =>
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
}
