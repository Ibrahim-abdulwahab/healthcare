using Healthcare.Api.DTOs.Schedules;
using Healthcare.Api.Services.Schedules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ScheduleDto>>> GetAll()
    {
        var schedules = await _scheduleService.GetAllAsync();

        return Ok(schedules);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> GetById(int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);

        if (schedule is null)
        {
            return NotFound();
        }

        return Ok(schedule);
    }

    [Authorize(Roles = "admin,doctor")]
    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> Create(
    CreateScheduleDto dto)
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
                await _scheduleService.DoctorOwnsLocationAsync(
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
            var schedule = await _scheduleService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = schedule.Id },
                schedule
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "admin,doctor")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> Update(
    int id,
    UpdateScheduleDto dto)
    {
        if (User.IsInRole("doctor"))
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var doctorId))
            {
                return Unauthorized();
            }

            var ownsSchedule =
                await _scheduleService.DoctorOwnsScheduleAsync(
                    doctorId,
                    id
                );

            if (!ownsSchedule)
            {
                return Forbid();
            }

            var ownsLocation =
                await _scheduleService.DoctorOwnsLocationAsync(
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
            var schedule =
                await _scheduleService.UpdateAsync(id, dto);

            if (schedule is null)
            {
                return NotFound();
            }

            return Ok(schedule);
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

            var ownsSchedule =
                await _scheduleService.DoctorOwnsScheduleAsync(
                    doctorId,
                    id
                );

            if (!ownsSchedule)
            {
                return Forbid();
            }
        }

        var deleted =
            await _scheduleService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}