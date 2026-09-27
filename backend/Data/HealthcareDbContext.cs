using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Security;

namespace Healthcare.Api.Data;

public class HealthcareDbContext : DbContext
{
    private readonly RequestUser? _actor;
    public int ActorId => _actor?.Id ?? 0;
    public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options, RequestUser? actor = null)
        : base(options)
    {
        _actor = actor;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DoctorLocation> DoctorLocations => Set<DoctorLocation>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleException> ScheduleExceptions => Set<ScheduleException>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VisitType> VisitTypes => Set<VisitType>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<LabRequest> LabRequests => Set<LabRequest>();
    public DbSet<ImagingRequest> ImagingRequests => Set<ImagingRequest>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // C# class HealthcareFile will later map to PostgreSQL table "files"
    public DbSet<HealthcareFile> Files => Set<HealthcareFile>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<CodeList> CodeLists => Set<CodeList>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_actor?.ExpectedUpdatedAt))
        {
            if (!DateTimeOffset.TryParse(_actor.ExpectedUpdatedAt, out var expected)) throw new ArgumentException("Invalid expected modification timestamp.");
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Metadata.FindProperty("UpdatedAt") != null))
                if ((DateTime)entry.Property("UpdatedAt").OriginalValue! != expected.UtcDateTime)
                    throw new Healthcare.Api.Infrastructure.ApiException(409, "This record changed. Refresh before editing.");
        }
        var changes = ChangeTracker.Entries().Where(e => e.Entity is not AuditLog &&
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new { Entry = e, Table = e.Metadata.GetTableName()!, Action = e.Entity is Appointment a && e.State == EntityState.Modified
                ? e.Property("Status").IsModified ? $"status:{a.Status}" : e.Property("StartDatetime").IsModified ? "rescheduled" : "Modified"
                : e.State.ToString() }).ToList();
        if (ActorId == 0 || changes.Count == 0) return await base.SaveChangesAsync(cancellationToken);
        await using var transaction = Database.CurrentTransaction == null ? await Database.BeginTransactionAsync(cancellationToken) : null;
        var count = await base.SaveChangesAsync(cancellationToken);
        foreach (var change in changes)
        {
            AuditLogs.Add(new AuditLog { UserId = ActorId, TableName = change.Table,
                RecordId = Convert.ToInt32(change.Entry.Property("Id").CurrentValue),
                Action = change.Action, CreatedAt = DateTime.UtcNow });
        }
        await base.SaveChangesAsync(cancellationToken);
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
        return count;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HealthcareDbContext).Assembly
        );
    }
}
