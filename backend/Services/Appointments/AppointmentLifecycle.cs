using Healthcare.Api.Infrastructure;

namespace Healthcare.Api.Services.Appointments;

public static class AppointmentLifecycle
{
    public static void Validate(string current, string next, bool hasCompletedVisit = false)
    {
        if (current == next) return;
        if (hasCompletedVisit) throw new ApiException(409, "Appointment status is locked because a visit is completed.");
        var allowed = current switch
        {
            "booked" => next is "arrived" or "cancelled",
            "arrived" => next is "waiting" or "cancelled",
            "waiting" => next is "in_consultation" or "cancelled",
            "in_consultation" => next is "completed" or "cancelled",
            _ => false
        };
        if (!allowed) throw new ApiException(409, $"Cannot change appointment from {current} to {next}.");
    }
}
