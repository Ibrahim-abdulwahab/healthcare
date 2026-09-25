using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(l => l.Address)
            .HasColumnName("address");


        builder.Property(l => l.City)
            .HasColumnName("city");


        builder.Property(l => l.Phone)
            .HasColumnName("phone");


        builder.Property(l => l.RoomNumber)
            .HasColumnName("room_number");


        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Organization → Locations
        builder.HasOne(l => l.Organization)
            .WithMany(o => o.Locations)
            .HasForeignKey(l => l.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}