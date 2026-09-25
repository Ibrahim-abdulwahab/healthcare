using Healthcare.Api.DTOs.FollowUps;
using Healthcare.Api.Services.FollowUps;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowUpsController : ControllerBase
{
    private readonly IFollowUpService _followUpService;

    public FollowUpsController(IFollowUpService followUpService)
    {
        _followUpService = followUpService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FollowUpDto>>> GetAll()
    {
        var followUps = await _followUpService.GetAllAsync();

        return Ok(followUps);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FollowUpDto>> GetById(int id)
    {
        var followUp = await _followUpService.GetByIdAsync(id);

        if (followUp is null)
            return NotFound();

        return Ok(followUp);
    }

    [HttpPost]
    public async Task<ActionResult<FollowUpDto>> Create(
        CreateFollowUpDto dto)
    {
        try
        {
            var followUp = await _followUpService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = followUp.Id },
                followUp
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FollowUpDto>> Update(
        int id,
        UpdateFollowUpDto dto)
    {
        try
        {
            var followUp = await _followUpService.UpdateAsync(id, dto);

            if (followUp is null)
                return NotFound();

            return Ok(followUp);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _followUpService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}