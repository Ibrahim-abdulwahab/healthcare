using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class LabRequestConfiguration : IEntityTypeConfiguration<LabRequest>
{
    public void Configure(EntityTypeBuilder<LabRequest> builder)
    {
        builder.ToTable("lab_requests", table =>
        {
            table.HasCheckConstraint(
                "ck_lab_requests_status",
                "\"status\" IN ('requested', 'completed')"
            );
        });

        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Id)
            .HasColumnName("id");

        builder.Property(lr => lr.VisitId)
            .HasColumnName("visit_id")
            .IsRequired();

        builder.Property(lr => lr.TestName)
            .HasColumnName("test_name")
            .IsRequired();

        builder.Property(lr => lr.Priority)
            .HasColumnName("priority");

        builder.Property(lr => lr.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(lr => lr.Notes)
            .HasColumnName("notes");

        builder.Property(lr => lr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(lr => lr.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Visit → Lab Requests
        builder.HasOne(lr => lr.Visit)
            .WithMany(v => v.LabRequests)
            .HasForeignKey(lr => lr.VisitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}