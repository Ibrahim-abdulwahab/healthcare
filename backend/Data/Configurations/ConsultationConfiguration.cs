using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("consultations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.VisitId)
            .HasColumnName("visit_id")
            .IsRequired();

        builder.Property(c => c.ReasonForVisit)
            .HasColumnName("reason_for_visit");

        builder.Property(c => c.Diagnosis)
            .HasColumnName("diagnosis");

        builder.Property(c => c.Notes)
            .HasColumnName("notes");

        builder.Property(c => c.FollowUpPlan)
            .HasColumnName("follow_up_plan");

        builder.Property(c => c.NextAppointmentId)
            .HasColumnName("next_appointment_id");

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Visit 1 → 1 Consultation
        builder.HasOne(c => c.Visit)
            .WithOne(v => v.Consultation)
            .HasForeignKey<Consultation>(c => c.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional next appointment
        builder.HasOne(c => c.NextAppointment)
            .WithMany()
            .HasForeignKey(c => c.NextAppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // User who created the consultation
        builder.HasOne(c => c.Creator)
            .WithMany(u => u.CreatedConsultations)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}