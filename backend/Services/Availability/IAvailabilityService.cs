using Healthcare.Api.DTOs.Availability;

namespace Healthcare.Api.Services.Availability;

public interface IAvailabilityService
{
    Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(
        int doctorId,
        int locationId,
        DateOnly date);
}