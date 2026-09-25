using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class DoctorLocationConfiguration : IEntityTypeConfiguration<DoctorLocation>
{
    public void Configure(EntityTypeBuilder<DoctorLocation> builder)
    {
        builder.ToTable("doctor_locations");

        builder.HasKey(dl => dl.Id);

        builder.Property(dl => dl.Id)
            .HasColumnName("id");

        builder.Property(dl => dl.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(dl => dl.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(dl => dl.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(dl => dl.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Doctor → Doctor Locations
        builder.HasOne(dl => dl.Doctor)
            .WithMany(d => d.DoctorLocations)
            .HasForeignKey(dl => dl.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);


        // Location → Doctor Locations
        builder.HasOne(dl => dl.Location)
            .WithMany(l => l.DoctorLocations)
            .HasForeignKey(dl => dl.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}