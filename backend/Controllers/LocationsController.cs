using Healthcare.Api.DTOs.Locations;
using Healthcare.Api.Services.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
    {
        if (User.IsInRole("admin"))
        {
            var locations = await _locationService.GetAllAsync();

            return Ok(locations);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (User.IsInRole("doctor"))
        {
            var locations =
                await _locationService.GetByDoctorIdAsync(userId);

            return Ok(locations);
        }

        if (User.IsInRole("staff"))
        {
            var assignment =
                await _locationService.GetStaffAssignmentAsync(userId);

            if (assignment.StaffDoctorId.HasValue)
            {
                var locations =
                    await _locationService.GetByDoctorIdAsync(
                        assignment.StaffDoctorId.Value
                    );

                return Ok(locations);
            }

            if (assignment.StaffOrganizationId.HasValue)
            {
                var locations =
                    await _locationService.GetByOrganizationIdAsync(
                        assignment.StaffOrganizationId.Value
                    );

                return Ok(locations);
            }

            return Ok(new List<LocationDto>());
        }

        return Forbid();
    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var location = await _locationService.GetByIdAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        if (User.IsInRole("admin"))
        {
            return Ok(location);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (User.IsInRole("doctor"))
        {
            var doctorLocations =
                await _locationService.GetByDoctorIdAsync(userId);

            if (!doctorLocations.Any(l => l.Id == id))
            {
                return Forbid();
            }

            return Ok(location);
        }

        if (User.IsInRole("staff"))
        {
            var assignment =
                await _locationService.GetStaffAssignmentAsync(userId);

            if (assignment.StaffDoctorId.HasValue)
            {
                var doctorLocations =
                    await _locationService.GetByDoctorIdAsync(
                        assignment.StaffDoctorId.Value
                    );

                if (!doctorLocations.Any(l => l.Id == id))
                {
                    return Forbid();
                }

                return Ok(location);
            }

            if (assignment.StaffOrganizationId.HasValue)
            {
                if (location.OrganizationId !=
                    assignment.StaffOrganizationId.Value)
                {
                    return Forbid();
                }

                return Ok(location);
            }

            return Forbid();
        }

        return Forbid();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(
        CreateLocationDto dto)
    {
        try
        {
            var location = await _locationService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = location.Id },
                location
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LocationDto>> Update(
        int id,
        UpdateLocationDto dto)
    {
        try
        {
            var location = await _locationService.UpdateAsync(id, dto);

            if (location is null)
            {
                return NotFound();
            }

            return Ok(location);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _locationService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}