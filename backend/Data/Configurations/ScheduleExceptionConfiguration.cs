using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ScheduleExceptionConfiguration
    : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ToTable("schedule_exceptions");

        builder.HasKey(se => se.Id);

        builder.Property(se => se.Id)
            .HasColumnName("id");

        builder.Property(se => se.DoctorLocationId)
            .HasColumnName("doctor_location_id")
            .IsRequired();

        builder.Property(se => se.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(se => se.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(se => se.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(se => se.IsAvailable)
            .HasColumnName("is_available")
            .IsRequired();

        builder.Property(se => se.Reason)
            .HasColumnName("reason");

        builder.Property(se => se.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(se => se.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Doctor Location → Schedule Exceptions
        builder.HasOne(se => se.DoctorLocation)
            .WithMany(dl => dl.ScheduleExceptions)
            .HasForeignKey(se => se.DoctorLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}