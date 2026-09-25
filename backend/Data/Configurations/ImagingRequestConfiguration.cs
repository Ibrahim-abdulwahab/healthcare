using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ImagingRequestConfiguration
    : IEntityTypeConfiguration<ImagingRequest>
{
    public void Configure(EntityTypeBuilder<ImagingRequest> builder)
    {
        builder.ToTable("imaging_requests", table =>
        {
            table.HasCheckConstraint(
                "ck_imaging_requests_status",
                "\"status\" IN ('requested', 'scheduled', 'completed')"
            );
        });

        builder.HasKey(ir => ir.Id);

        builder.Property(ir => ir.Id)
            .HasColumnName("id");

        builder.Property(ir => ir.VisitId)
            .HasColumnName("visit_id")
            .IsRequired();

        builder.Property(ir => ir.ServiceName)
            .HasColumnName("service_name")
            .IsRequired();

        builder.Property(ir => ir.BodyPart)
            .HasColumnName("body_part");

        builder.Property(ir => ir.Priority)
            .HasColumnName("priority");

        builder.Property(ir => ir.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(ir => ir.ScheduledDate)
            .HasColumnName("scheduled_date")
            .HasColumnType("date");

        builder.Property(ir => ir.Notes)
            .HasColumnName("notes");

        builder.Property(ir => ir.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ir => ir.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Visit → Imaging Requests
        builder.HasOne(ir => ir.Visit)
            .WithMany(v => v.ImagingRequests)
            .HasForeignKey(ir => ir.VisitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}