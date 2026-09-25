using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ItemCategoryConfiguration
    : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("item_categories");

        builder.HasKey(ic => ic.Id);

        builder.Property(ic => ic.Id)
            .HasColumnName("id");

        builder.Property(ic => ic.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(ic => ic.Description)
            .HasColumnName("description");

        builder.Property(ic => ic.ParentId)
            .HasColumnName("parent_id");

        builder.Property(ic => ic.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ic => ic.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ic => ic.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Self-referencing category hierarchy
        builder.HasOne(ic => ic.Parent)
            .WithMany(ic => ic.Children)
            .HasForeignKey(ic => ic.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}