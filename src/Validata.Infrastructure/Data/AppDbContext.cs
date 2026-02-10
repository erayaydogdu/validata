using Microsoft.EntityFrameworkCore;
using Validata.Core.Entities;

namespace Validata.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<ScreeningRequest> ScreeningRequests => Set<ScreeningRequest>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<VerificationStep> VerificationSteps => Set<VerificationStep>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<GDPRRequest> GDPRRequests => Set<GDPRRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.Users)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<ScreeningRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Candidate)
                  .WithMany(c => c.ScreeningRequests)
                  .HasForeignKey(e => e.CandidateId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.ScreeningRequests)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.CreatedScreenings)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedVerifier)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedVerifierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ScreeningRequest)
                  .WithMany(s => s.Documents)
                  .HasForeignKey(e => e.ScreeningRequestId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Candidate)
                  .WithMany(c => c.Documents)
                  .HasForeignKey(e => e.CandidateId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.UploadedBy)
                  .WithMany(u => u.UploadedDocuments)
                  .HasForeignKey(e => e.UploadedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VerificationStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ScreeningRequest)
                  .WithMany(s => s.VerificationSteps)
                  .HasForeignKey(e => e.ScreeningRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedTo)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<GDPRRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Candidate)
                  .WithMany(c => c.GDPRRequests)
                  .HasForeignKey(e => e.CandidateId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CompletedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CompletedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
