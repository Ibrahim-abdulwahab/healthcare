using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class VisitTypeConfiguration : IEntityTypeConfiguration<VisitType>
{
    public void Configure(EntityTypeBuilder<VisitType> builder)
    {
        builder.ToTable("visit_types");

        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Id)
            .HasColumnName("id");

        builder.Property(vt => vt.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(vt => vt.Description)
            .HasColumnName("description");

        builder.Property(vt => vt.DefaultDuration)
            .HasColumnName("default_duration")
            .IsRequired();

        builder.Property(vt => vt.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(vt => vt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(vt => vt.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}