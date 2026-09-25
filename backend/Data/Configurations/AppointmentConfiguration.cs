using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments", table =>
        {
            table.HasCheckConstraint(
                "ck_appointments_status",
                "\"status\" IN ('booked', 'arrived', 'waiting', 'in_consultation', 'completed', 'cancelled')"
            );
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(a => a.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(a => a.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(a => a.VisitTypeId)
            .HasColumnName("visit_type_id")
            .IsRequired();

        builder.Property(a => a.StartDatetime)
            .HasColumnName("start_datetime")
            .IsRequired();

        builder.Property(a => a.EndDatetime)
            .HasColumnName("end_datetime")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasColumnName("notes");

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Patient → Appointments
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor → Appointments
        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Location → Appointments
        builder.HasOne(a => a.Location)
            .WithMany(l => l.Appointments)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Visit Type → Appointments
        builder.HasOne(a => a.VisitType)
            .WithMany(vt => vt.Appointments)
            .HasForeignKey(a => a.VisitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // User → Appointments created by that user
        builder.HasOne(a => a.Creator)
            .WithMany(u => u.CreatedAppointments)
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}