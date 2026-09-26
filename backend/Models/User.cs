namespace Healthcare.Api.Models;

public class User
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    // Allowed values: admin, doctor, staff
    public required string Role { get; set; }

    public Doctor? Doctor { get; set; }

    public Doctor? StaffDoctor { get; set; }
    public Organization? StaffOrganization { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int? StaffDoctorId { get; set; }

    public int? StaffOrganizationId { get; set; }


    // Relationships

    public ICollection<PaymentRecord> CollectedPayments { get; set; } = [];

    public ICollection<Appointment> CreatedAppointments { get; set; } = [];

    public ICollection<Consultation> CreatedConsultations { get; set; } = [];

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<Notification> Notifications { get; set; } = [];

    public ICollection<HealthcareFile> UploadedFiles { get; set; } = [];

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}