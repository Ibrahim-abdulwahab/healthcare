using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("visits", table =>
        {
            table.HasCheckConstraint(
                "ck_visits_status",
                "\"status\" IN ('in_progress', 'completed')"
            );
        });

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.AppointmentId)
            .HasColumnName("appointment_id")
            .IsRequired();

        builder.Property(v => v.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(v => v.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(v => v.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(v => v.CheckInTime)
            .HasColumnName("check_in_time");

        builder.Property(v => v.CheckOutTime)
            .HasColumnName("check_out_time");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Appointment 1 → N Visits
        builder.HasOne(v => v.Appointment)
            .WithMany(a => a.Visits)
            .HasForeignKey(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient → Visits
        builder.HasOne(v => v.Patient)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor → Visits
        builder.HasOne(v => v.Doctor)
            .WithMany(d => d.Visits)
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Location → Visits
        builder.HasOne(v => v.Location)
            .WithMany(l => l.Visits)
            .HasForeignKey(v => v.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}