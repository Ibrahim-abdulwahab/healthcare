using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Mrn)
            .HasColumnName("mrn")
            .IsRequired();

        builder.Property(p => p.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(p => p.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(p => p.Sex)
            .HasColumnName("sex");

        builder.Property(p => p.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date");

        builder.Property(p => p.Phone)
            .HasColumnName("phone");

        builder.Property(p => p.Email)
            .HasColumnName("email");

        builder.Property(p => p.Address)
            .HasColumnName("address");

        builder.Property(p => p.NationalId)
            .HasColumnName("national_id");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // MRN must be unique
        builder.HasIndex(p => p.Mrn)
            .IsUnique();
    }
}