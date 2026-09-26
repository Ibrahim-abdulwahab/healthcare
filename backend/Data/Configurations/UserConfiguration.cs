using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.StaffDoctorId)
            .HasColumnName("staff_doctor_id");

        builder.Property(u => u.StaffOrganizationId)
            .HasColumnName("staff_organization_id");

        builder.HasOne(u => u.StaffDoctor)
            .WithMany()
            .HasForeignKey(u => u.StaffDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.StaffOrganization)
            .WithMany()
            .HasForeignKey(u => u.StaffOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint(
                "ck_users_role",
                "\"role\" IN ('admin', 'doctor', 'staff')"
            );
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // users.email must be unique
        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}