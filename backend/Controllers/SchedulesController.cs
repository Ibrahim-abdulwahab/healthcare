using Healthcare.Api.DTOs.Schedules;
using Healthcare.Api.Services.Schedules;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> Create(
        CreateScheduleDto dto)
    {
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> Update(
        int id,
        UpdateScheduleDto dto)
    {
        try
        {
            var schedule = await _scheduleService.UpdateAsync(id, dto);

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _scheduleService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}