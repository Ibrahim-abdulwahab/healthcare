using Healthcare.Api.DTOs.Availability;
using Healthcare.Api.Services.Availability;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(
        IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots(
        [FromQuery] int doctorId,
        [FromQuery] int locationId,
        [FromQuery] DateOnly date)
    {
        var slots = await _availabilityService.GetAvailableSlotsAsync(
            doctorId,
            locationId,
            date);

        return Ok(slots);
    }
}