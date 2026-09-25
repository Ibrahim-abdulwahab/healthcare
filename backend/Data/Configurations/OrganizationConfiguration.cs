using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations", table =>
        {
            table.HasCheckConstraint(
                "ck_organizations_type",
                "\"type\" IN ('hospital', 'clinic')"
            );
        });

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(o => o.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(o => o.Address)
            .HasColumnName("address");


        builder.Property(o => o.Phone)
            .HasColumnName("phone");


        builder.Property(o => o.Email)
            .HasColumnName("email");


        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}