using Healthcare.Api.DTOs.VisitTypes;
using Healthcare.Api.Services.VisitTypes;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/visit-types")]
public class VisitTypesController : ControllerBase
{
    private readonly IVisitTypeService _visitTypeService;

    public VisitTypesController(IVisitTypeService visitTypeService)
    {
        _visitTypeService = visitTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitTypeDto>>> GetAll()
    {
        var visitTypes = await _visitTypeService.GetAllAsync();
        return Ok(visitTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VisitTypeDto>> GetById(int id)
    {
        var visitType = await _visitTypeService.GetByIdAsync(id);

        if (visitType is null)
        {
            return NotFound();
        }

        return Ok(visitType);
    }

    [HttpPost]
    public async Task<ActionResult<VisitTypeDto>> Create(
        CreateVisitTypeDto dto)
    {
        try
        {
            var visitType = await _visitTypeService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = visitType.Id },
                visitType
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VisitTypeDto>> Update(
        int id,
        UpdateVisitTypeDto dto)
    {
        try
        {
            var visitType = await _visitTypeService.UpdateAsync(id, dto);

            if (visitType is null)
            {
                return NotFound();
            }

            return Ok(visitType);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _visitTypeService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}