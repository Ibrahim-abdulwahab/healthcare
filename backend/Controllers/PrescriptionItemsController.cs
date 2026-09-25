using Healthcare.Api.DTOs.PrescriptionItems;
using Healthcare.Api.Services.PrescriptionItems;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionItemsController : ControllerBase
{
    private readonly IPrescriptionItemService _prescriptionItemService;

    public PrescriptionItemsController(
        IPrescriptionItemService prescriptionItemService)
    {
        _prescriptionItemService = prescriptionItemService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrescriptionItemDto>>> GetAll()
    {
        var prescriptionItems =
            await _prescriptionItemService.GetAllAsync();

        return Ok(prescriptionItems);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrescriptionItemDto>> GetById(int id)
    {
        var prescriptionItem =
            await _prescriptionItemService.GetByIdAsync(id);

        if (prescriptionItem is null)
            return NotFound();

        return Ok(prescriptionItem);
    }

    [HttpPost]
    public async Task<ActionResult<PrescriptionItemDto>> Create(
        CreatePrescriptionItemDto dto)
    {
        try
        {
            var prescriptionItem =
                await _prescriptionItemService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = prescriptionItem.Id },
                prescriptionItem
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PrescriptionItemDto>> Update(
        int id,
        UpdatePrescriptionItemDto dto)
    {
        try
        {
            var prescriptionItem =
                await _prescriptionItemService.UpdateAsync(id, dto);

            if (prescriptionItem is null)
                return NotFound();

            return Ok(prescriptionItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _prescriptionItemService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}