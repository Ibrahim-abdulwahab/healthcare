using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules", table =>
        {
            table.HasCheckConstraint(
                "ck_schedules_day_of_week",
                "\"day_of_week\" >= 0 AND \"day_of_week\" <= 6"
            );
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.DoctorLocationId)
            .HasColumnName("doctor_location_id")
            .IsRequired();

        builder.Property(s => s.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        builder.Property(s => s.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(s => s.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(s => s.SlotDuration)
            .HasColumnName("slot_duration")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();


        // Doctor Location → Schedules
        builder.HasOne(s => s.DoctorLocation)
            .WithMany(dl => dl.Schedules)
            .HasForeignKey(s => s.DoctorLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}