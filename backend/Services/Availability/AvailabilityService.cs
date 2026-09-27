using Healthcare.Api.Configuration;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Availability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services.Availability;

public class AvailabilityService(HealthcareDbContext context, IOptions<HealthcareSettings> options) : IAvailabilityService
{
    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId, int locationId, DateOnly date, int visitTypeId, int? excludeAppointmentId = null)
    {
        if (doctorId <= 0 || locationId <= 0 || visitTypeId <= 0 || date.Year is < 2 or > 9998)
            throw new ArgumentException("A doctor, location, visit type and valid calendar date are required.");
        var duration = await context.VisitTypes.Where(v => v.Id == visitTypeId && v.IsActive).Select(v => (int?)v.DefaultDuration).SingleOrDefaultAsync();
        if (duration is null) return [];
        // Read existing duplicate mappings together; reject new active duplicates on write.
        var mappings = context.DoctorLocations.Where(dl => dl.DoctorId == doctorId && dl.LocationId == locationId && dl.IsActive && dl.Doctor.IsActive && dl.Location.IsActive).Select(dl => dl.Id);
        var schedules = await context.Schedules.AsNoTracking().Where(s => mappings.Contains(s.DoctorLocationId) && s.IsActive).ToListAsync();
        var exceptions = await context.ScheduleExceptions.AsNoTracking().Where(s => mappings.Contains(s.DoctorLocationId) && s.Date == date).ToListAsync();
        var weekday = (int)date.DayOfWeek;
        var working = schedules.Where(s => s.DayOfWeek == weekday).Select(s => new SlotCalculator.Window(s.StartTime, s.EndTime, s.SlotDuration)).ToList();
        // Extra hours use the same weekday's smallest interval, otherwise the mapping's smallest interval,
        // otherwise the selected visit duration. Starts are anchored at the extra-hours interval start.
        foreach (var extra in exceptions.Where(e => e.IsAvailable))
        {
            var related = schedules.Where(s => s.DoctorLocationId == extra.DoctorLocationId && s.SlotDuration > 0 && s.SlotDuration < 1440).ToList();
            var step = related.Where(s => s.DayOfWeek == weekday).Select(s => (int?)s.SlotDuration).Min()
                ?? related.Select(s => (int?)s.SlotDuration).Min() ?? duration.Value;
            working.Add(new(extra.StartTime, extra.EndTime, step));
        }
        var blocked = exceptions.Where(e => !e.IsAvailable).Select(e => new SlotCalculator.Window(e.StartTime, e.EndTime, 1));
        var from = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue).AddDays(-1), DateTimeKind.Utc);
        var to = from.AddDays(3);
        // Deliberately spans every location for this doctor; public output contains times only.
        var occupied = await context.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.Status != "cancelled" && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value) && a.StartDatetime < to && a.EndDatetime > from)
            .Select(a => new SlotCalculator.Busy(a.StartDatetime, a.EndDatetime)).ToListAsync();
        return SlotCalculator.Calculate(date, duration.Value, TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone), working, blocked, occupied);
    }
}
