using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items", table =>
        {
            table.HasCheckConstraint(
                "ck_items_type",
                "\"type\" IN ('medication', 'supply')"
            );
        });

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(i => i.GenericName)
            .HasColumnName("generic_name");

        builder.Property(i => i.BrandName)
            .HasColumnName("brand_name");

        builder.Property(i => i.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(i => i.Unit)
            .HasColumnName("unit")
            .IsRequired();

        builder.Property(i => i.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Item code must be unique
        builder.HasIndex(i => i.Code)
            .IsUnique();


        // Item Category → Items
        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}