using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class CodeListConfiguration
    : IEntityTypeConfiguration<CodeList>
{
    public void Configure(EntityTypeBuilder<CodeList> builder)
    {
        builder.ToTable("code_lists");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Category)
            .HasColumnName("category")
            .IsRequired();

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(c => c.ValueEn)
            .HasColumnName("value_en")
            .IsRequired();

        builder.Property(c => c.ValueAr)
            .HasColumnName("value_ar");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}