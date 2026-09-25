using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.ToTable("follow_ups", table =>
        {
            table.HasCheckConstraint(
                "ck_follow_ups_status",
                "\"status\" IN ('open', 'completed')"
            );
        });

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(f => f.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(f => f.AppointmentId)
            .HasColumnName("appointment_id");

        builder.Property(f => f.Notes)
            .HasColumnName("notes");

        builder.Property(f => f.ReminderDate)
            .HasColumnName("reminder_date")
            .HasColumnType("date");

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Patient → Follow Ups
        builder.HasOne(f => f.Patient)
            .WithMany(p => p.FollowUps)
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor → Follow Ups
        builder.HasOne(f => f.Doctor)
            .WithMany(d => d.FollowUps)
            .HasForeignKey(f => f.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment → Follow Ups
        builder.HasOne(f => f.Appointment)
            .WithMany(a => a.FollowUps)
            .HasForeignKey(f => f.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}