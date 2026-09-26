using Healthcare.Api.DTOs.ScheduleExceptions;
using Healthcare.Api.Services.ScheduleExceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/schedule-exceptions")]
public class ScheduleExceptionsController : ControllerBase
{
    private readonly IScheduleExceptionService _scheduleExceptionService;

    public ScheduleExceptionsController(
        IScheduleExceptionService scheduleExceptionService)
    {
        _scheduleExceptionService = scheduleExceptionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ScheduleExceptionDto>>> GetAll()
    {
        var scheduleExceptions =
            await _scheduleExceptionService.GetAllAsync();

        return Ok(scheduleExceptions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduleExceptionDto>> GetById(int id)
    {
        var scheduleException =
            await _scheduleExceptionService.GetByIdAsync(id);

        if (scheduleException is null)
        {
            return NotFound();
        }

        return Ok(scheduleException);
    }

    [Authorize(Roles = "admin,doctor")]
    [HttpPost]
    public async Task<ActionResult<ScheduleExceptionDto>> Create(
    CreateScheduleExceptionDto dto)
    {
        if (User.IsInRole("doctor"))
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var doctorId))
            {
                return Unauthorized();
            }

            var ownsLocation =
                await _scheduleExceptionService.DoctorOwnsLocationAsync(
                    doctorId,
                    dto.DoctorLocationId
                );

            if (!ownsLocation)
            {
                return Forbid();
            }
        }

        try
        {
            var scheduleException =
                await _scheduleExceptionService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = scheduleException.Id },
                scheduleException
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin,doctor")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleExceptionDto>> Update(
    int id,
    UpdateScheduleExceptionDto dto)
    {
        if (User.IsInRole("doctor"))
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var doctorId))
            {
                return Unauthorized();
            }

            var ownsException =
                await _scheduleExceptionService.DoctorOwnsExceptionAsync(
                    doctorId,
                    id
                );

            if (!ownsException)
            {
                return Forbid();
            }

            var ownsLocation =
                await _scheduleExceptionService.DoctorOwnsLocationAsync(
                    doctorId,
                    dto.DoctorLocationId
                );

            if (!ownsLocation)
            {
                return Forbid();
            }
        }

        try
        {
            var scheduleException =
                await _scheduleExceptionService.UpdateAsync(id, dto);

            if (scheduleException is null)
            {
                return NotFound();
            }

            return Ok(scheduleException);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin,doctor")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (User.IsInRole("doctor"))
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var doctorId))
            {
                return Unauthorized();
            }

            var ownsException =
                await _scheduleExceptionService.DoctorOwnsExceptionAsync(
                    doctorId,
                    id
                );

            if (!ownsException)
            {
                return Forbid();
            }
        }

        var deleted =
            await _scheduleExceptionService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}