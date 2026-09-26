using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.Services.Appointments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
    {
        if (User.IsInRole("admin"))
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (User.IsInRole("doctor"))
        {
            var doctorAppointments =
                await _appointmentService.GetByDoctorIdAsync(userId);

            return Ok(doctorAppointments);
        }

        if (User.IsInRole("staff"))
        {
            var staffAppointments =
                await _appointmentService.GetByStaffUserIdAsync(userId);

            return Ok(staffAppointments);
        }

        return Forbid();


    }

    [Authorize(Roles = "admin,doctor,staff")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment is null)
        {
            return NotFound();
        }

        // Admin can access any appointment
        if (User.IsInRole("admin"))
        {
            return Ok(appointment);
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Doctor can access only their own appointments
        if (User.IsInRole("doctor"))
        {
            if (appointment.DoctorId != userId)
            {
                return Forbid();
            }

            return Ok(appointment);
        }

        // Staff can access appointments only for their
        // assigned doctor or organization
        if (User.IsInRole("staff"))
        {
            var canManage =
                await _appointmentService.StaffCanManageDoctorAsync(
                    userId,
                    appointment.DoctorId
                );

            if (!canManage)
            {
                return Forbid();
            }

            return Ok(appointment);
        }

        return Forbid();
    }

    [Authorize(Roles = "admin,staff")]
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
    CreateAppointmentDto dto)
    {
        try
        {
            var userIdClaim =
    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var appointment =
                await _appointmentService.CreateAsync(
                    dto,
                    currentUserId
                );

            return CreatedAtAction(
                nameof(GetById),
                new { id = appointment.Id },
                appointment
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "admin,staff")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> Update(
    int id,
    UpdateAppointmentDto dto)
    {
        try
        {
            if (User.IsInRole("staff"))
            {
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var staffUserId))
                {
                    return Unauthorized();
                }

                // Check the existing appointment
                var existingAppointment =
                    await _appointmentService.GetByIdAsync(id);

                if (existingAppointment is null)
                {
                    return NotFound();
                }

                var canManageExisting =
                    await _appointmentService.StaffCanManageDoctorAsync(
                        staffUserId,
                        existingAppointment.DoctorId
                    );

                if (!canManageExisting)
                {
                    return Forbid();
                }

                // Check the doctor submitted in the update
                var canManageNewDoctor =
                    await _appointmentService.StaffCanManageDoctorAsync(
                        staffUserId,
                        dto.DoctorId
                    );

                if (!canManageNewDoctor)
                {
                    return Forbid();
                }
            }

            var appointment =
                await _appointmentService.UpdateAsync(id, dto);

            if (appointment is null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _appointmentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}