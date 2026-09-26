using Healthcare.Api.DTOs.Doctors;
using Healthcare.Api.Services.Doctors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> GetAll()
    {
        if (User.IsInRole("admin"))
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (User.IsInRole("doctor"))
        {
            var doctor = await _doctorService.GetByIdAsync(userId);

            if (doctor is null)
            {
                return Ok(new List<DoctorDto>());
            }

            return Ok(new List<DoctorDto> { doctor });
        }

        if (User.IsInRole("staff"))
        {
            // Staff assigned directly to one doctor
            var assignedDoctor =
                await _doctorService.GetByStaffUserIdAsync(userId);

            if (assignedDoctor is not null)
            {
                return Ok(new List<DoctorDto> { assignedDoctor });
            }

            // Staff assigned to an organization
            var organizationId =
                await _doctorService.GetStaffOrganizationIdAsync(userId);

            if (organizationId.HasValue)
            {
                var doctors =
                    await _doctorService.GetByOrganizationIdAsync(
                        organizationId.Value
                    );

                return Ok(doctors);
            }

            return Ok(new List<DoctorDto>());
        }

        return Forbid();
    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDto>> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor is null)
        {
            return NotFound();
        }

        if (User.IsInRole("admin"))
        {
            return Ok(doctor);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (User.IsInRole("doctor"))
        {
            if (userId != id)
            {
                return Forbid();
            }

            return Ok(doctor);
        }

        if (User.IsInRole("staff"))
        {
            var canAccess =
                await _doctorService.StaffCanAccessDoctorAsync(
                    userId,
                    id
                );

            if (!canAccess)
            {
                return Forbid();
            }

            return Ok(doctor);
        }

        return Forbid();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<DoctorDto>> Create(
        CreateDoctorDto dto)
    {
        try
        {
            var doctor = await _doctorService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = doctor.Id },
                doctor
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DoctorDto>> Update(
        int id,
        UpdateDoctorDto dto)
    {
        try
        {
            var doctor = await _doctorService.UpdateAsync(id, dto);

            if (doctor is null)
            {
                return NotFound();
            }

            return Ok(doctor);
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
        var deleted = await _doctorService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}