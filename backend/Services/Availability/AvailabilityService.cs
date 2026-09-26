using Healthcare.Api.Configuration;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Availability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services.Availability;

public class AvailabilityService : IAvailabilityService
{
    private readonly HealthcareDbContext _context;
    private readonly HealthcareSettings _settings;

    public AvailabilityService(
        HealthcareDbContext context,
        IOptions<HealthcareSettings> options)
    {
        _context = context;
        _settings = options.Value;
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(
        int doctorId,
        int locationId,
        DateOnly date)
    {
        // Find the active relationship between the doctor and location.
        var doctorLocation = await _context.DoctorLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(dl =>
                dl.DoctorId == doctorId &&
                dl.LocationId == locationId &&
                dl.IsActive);

        if (doctorLocation is null)
        {
            return [];
        }

        // Find the doctor's active schedules for the requested weekday.
        var dayOfWeek = (int)date.DayOfWeek;

        var schedules = await _context.Schedules
            .AsNoTracking()
            .Where(s =>
                s.DoctorLocationId == doctorLocation.Id &&
                s.DayOfWeek == dayOfWeek &&
                s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // Find schedule exceptions for this exact date.
        var exceptions = await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(se =>
                se.DoctorLocationId == doctorLocation.Id &&
                se.Date == date)
            .ToListAsync();

        var unavailableExceptions = exceptions
            .Where(se => !se.IsAvailable)
            .ToList();

        // All schedule times are interpreted using the
        // application's configured timezone.
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            _settings.TimeZone);

        // Convert the requested local calendar day into its
        // corresponding UTC boundaries.
        var localDayStart = DateTime.SpecifyKind(
            date.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);

        var localNextDayStart = DateTime.SpecifyKind(
            date.AddDays(1).ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Unspecified);

        var utcDayStart = TimeZoneInfo.ConvertTimeToUtc(
            localDayStart,
            timeZone);

        var utcNextDayStart = TimeZoneInfo.ConvertTimeToUtc(
            localNextDayStart,
            timeZone);

        // Load appointments that occupy part of this local calendar day.
        //
        // Cancelled appointments do not block availability.
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.DoctorId == doctorId &&
                a.LocationId == locationId &&
                a.Status != "cancelled" &&
                a.StartDatetime < utcNextDayStart &&
                a.EndDatetime > utcDayStart)
            .ToListAsync();

        var slots = new List<AvailableSlotDto>();

        // Generate candidate slots from each regular schedule.
        foreach (var schedule in schedules)
        {
            var currentTime = schedule.StartTime;

            while (
                currentTime.AddMinutes(schedule.SlotDuration)
                <= schedule.EndTime)
            {
                var endTime =
                    currentTime.AddMinutes(schedule.SlotDuration);

                // Remove slots that overlap an unavailable
                // ScheduleException.
                var overlapsUnavailableException =
                    unavailableExceptions.Any(se =>
                        currentTime < se.EndTime &&
                        endTime > se.StartTime);

                if (overlapsUnavailableException)
                {
                    currentTime = endTime;
                    continue;
                }

                // Schedule times represent local application time.
                var localStart = DateTime.SpecifyKind(
                    date.ToDateTime(currentTime),
                    DateTimeKind.Unspecified);

                var localEnd = DateTime.SpecifyKind(
                    date.ToDateTime(endTime),
                    DateTimeKind.Unspecified);

                // Convert them to UTC so they can be compared with
                // Appointment timestamps stored as timestamptz.
                var utcStart = TimeZoneInfo.ConvertTimeToUtc(
                    localStart,
                    timeZone);

                var utcEnd = TimeZoneInfo.ConvertTimeToUtc(
    localEnd,
    timeZone);

                var overlapsAppointment =
                    appointments.Any(a =>
                        utcStart < a.EndDatetime &&
                        utcEnd > a.StartDatetime);

                if (overlapsAppointment)
                {
                    currentTime = endTime;
                    continue;
                }

                slots.Add(new AvailableSlotDto
                {
                    StartDatetime = utcStart,
                    EndDatetime = utcEnd
                });

                currentTime = endTime;
            }
        }

        return slots;
    }
}