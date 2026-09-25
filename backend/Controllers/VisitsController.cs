using Healthcare.Api.DTOs.Visits;
using Healthcare.Api.Services.Visits;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitsController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitDto>>> GetAll()
    {
        var visits = await _visitService.GetAllAsync();

        return Ok(visits);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VisitDto>> GetById(int id)
    {
        var visit = await _visitService.GetByIdAsync(id);

        if (visit is null)
            return NotFound();

        return Ok(visit);
    }

    [HttpPost]
    public async Task<ActionResult<VisitDto>> Create(CreateVisitDto dto)
    {
        try
        {
            var visit = await _visitService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = visit.Id },
                visit
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VisitDto>> Update(
        int id,
        UpdateVisitDto dto)
    {
        try
        {
            var visit = await _visitService.UpdateAsync(id, dto);

            if (visit is null)
                return NotFound();

            return Ok(visit);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _visitService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}