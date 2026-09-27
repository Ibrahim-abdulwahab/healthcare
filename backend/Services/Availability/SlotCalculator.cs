using Healthcare.Api.DTOs.Availability;

namespace Healthcare.Api.Services.Availability;

public static class SlotCalculator
{
    public record Window(TimeOnly Start, TimeOnly End, int Step);
    public record Busy(DateTime Start, DateTime End);

    public static List<AvailableSlotDto> Calculate(DateOnly date, int duration, TimeZoneInfo zone,
        IEnumerable<Window> working, IEnumerable<Window> blocked, IEnumerable<Busy> occupied)
    {
        if (duration <= 0 || duration >= 1440) throw new ArgumentException("Visit duration must be between 1 and 1439 minutes.");
        var windows = working.Where(w => w.End > w.Start && w.Step > 0 && w.Step < 1440).ToList();
        var exclusions = blocked.ToList();
        var busy = occupied.ToList();
        var merged = new List<(DateTime Start, DateTime End)>();
        foreach (var window in windows.OrderBy(w => w.Start))
        {
            var start = date.ToDateTime(window.Start);
            var end = date.ToDateTime(window.End);
            if (merged.Count > 0 && start <= merged[^1].End)
                merged[^1] = (merged[^1].Start, end > merged[^1].End ? end : merged[^1].End);
            else merged.Add((start, end));
        }
        var slots = new SortedDictionary<DateTime, AvailableSlotDto>();
        foreach (var window in windows)
        {
            // DateTime arithmetic never wraps to an earlier clock time.
            for (var localStart = date.ToDateTime(window.Start); localStart < date.ToDateTime(window.End); localStart = localStart.AddMinutes(window.Step))
            {
                var localEnd = localStart.AddMinutes(duration);
                if (!merged.Any(w => localStart >= w.Start && localEnd <= w.End)) continue;
                if (exclusions.Any(w => localStart < date.ToDateTime(w.End) && localEnd > date.ToDateTime(w.Start))) continue;
                // Omit nonexistent/ambiguous wall times rather than silently choosing a DST offset.
                if (zone.IsInvalidTime(localStart) || zone.IsInvalidTime(localEnd) || zone.IsAmbiguousTime(localStart) || zone.IsAmbiguousTime(localEnd)) continue;
                var start = TimeZoneInfo.ConvertTimeToUtc(localStart, zone);
                var end = TimeZoneInfo.ConvertTimeToUtc(localEnd, zone);
                if (end - start != TimeSpan.FromMinutes(duration)) continue;
                if (busy.Any(b => b.Start < end && b.End > start)) continue;
                slots[start] = new AvailableSlotDto { StartDatetime = start, EndDatetime = end };
            }
        }
        return slots.Values.ToList();
    }
}
