using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Data;

public class HealthcareDbContext : DbContext
{
    public HealthcareDbContext(DbContextOptions<HealthcareDbContext> options)
        : base(options)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HealthcareDbContext).Assembly
        );
    }
}