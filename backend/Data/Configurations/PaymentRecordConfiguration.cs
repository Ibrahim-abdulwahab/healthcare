using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class PaymentRecordConfiguration
    : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable("payment_records", table =>
        {
            table.HasCheckConstraint(
                "ck_payment_records_payment_method",
                "\"payment_method\" IN ('cash', 'card', 'transfer', 'insurance')"
            );

            table.HasCheckConstraint(
                "ck_payment_records_paid_to",
                "\"paid_to\" IN ('hospital', 'doctor')"
            );

            table.HasCheckConstraint(
                "ck_payment_records_payment_status",
                "\"payment_status\" IN ('paid', 'partial', 'unpaid')"
            );
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.VisitId)
            .HasColumnName("visit_id")
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasColumnName("payment_method")
            .IsRequired();

        builder.Property(p => p.CollectedBy)
            .HasColumnName("collected_by")
            .IsRequired();

        builder.Property(p => p.PaidTo)
            .HasColumnName("paid_to")
            .IsRequired();

        builder.Property(p => p.PaymentStatus)
            .HasColumnName("payment_status")
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasColumnName("notes");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Visit → Payment Records
        builder.HasOne(p => p.Visit)
            .WithMany(v => v.PaymentRecords)
            .HasForeignKey(p => p.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        // User → Payments collected by that user
        builder.HasOne(p => p.Collector)
            .WithMany(u => u.CollectedPayments)
            .HasForeignKey(p => p.CollectedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}