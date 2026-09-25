using Healthcare.Api.DTOs.ScheduleExceptions;
using Healthcare.Api.Services.ScheduleExceptions;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<ActionResult<ScheduleExceptionDto>> Create(
        CreateScheduleExceptionDto dto)
    {
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleExceptionDto>> Update(
        int id,
        UpdateScheduleExceptionDto dto)
    {
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _scheduleExceptionService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}