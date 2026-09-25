using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class PrescriptionItemConfiguration
    : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .HasColumnName("id");

        builder.Property(pi => pi.PrescriptionId)
            .HasColumnName("prescription_id")
            .IsRequired();

        builder.Property(pi => pi.ItemId)
            .HasColumnName("item_id")
            .IsRequired();

        builder.Property(pi => pi.Dosage)
            .HasColumnName("dosage");

        builder.Property(pi => pi.Frequency)
            .HasColumnName("frequency");

        builder.Property(pi => pi.DurationDays)
            .HasColumnName("duration_days");

        builder.Property(pi => pi.Quantity)
            .HasColumnName("quantity");

        builder.Property(pi => pi.Route)
            .HasColumnName("route");

        builder.Property(pi => pi.Instructions)
            .HasColumnName("instructions");

        builder.Property(pi => pi.Notes)
            .HasColumnName("notes");

        builder.Property(pi => pi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pi => pi.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Prescription → Prescription Items
        builder.HasOne(pi => pi.Prescription)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Item → Prescription Items
        builder.HasOne(pi => pi.Item)
            .WithMany(i => i.PrescriptionItems)
            .HasForeignKey(pi => pi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}