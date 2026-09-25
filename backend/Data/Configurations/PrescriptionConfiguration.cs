using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions", table =>
        {
            table.HasCheckConstraint(
                "ck_prescriptions_status",
                "\"status\" IN ('active', 'canceled')"
            );
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.ConsultationId)
            .HasColumnName("consultation_id")
            .IsRequired();

        builder.Property(p => p.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(p => p.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(p => p.PrescriptionDate)
            .HasColumnName("prescription_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasColumnName("notes");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Consultation → Prescriptions
        builder.HasOne(p => p.Consultation)
            .WithMany(c => c.Prescriptions)
            .HasForeignKey(p => p.ConsultationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient → Prescriptions
        builder.HasOne(p => p.Patient)
            .WithMany(patient => patient.Prescriptions)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor → Prescriptions
        builder.HasOne(p => p.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}