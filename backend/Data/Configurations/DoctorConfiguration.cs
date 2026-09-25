using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(d => d.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(d => d.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(d => d.Specialty)
            .HasColumnName("specialty");

        builder.Property(d => d.Phone)
            .HasColumnName("phone");

        builder.Property(d => d.Email)
            .HasColumnName("email");

        builder.Property(d => d.LicenseNumber)
            .HasColumnName("license_number");

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Organization → Doctors
        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Doctors)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}