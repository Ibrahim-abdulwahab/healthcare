using Healthcare.Api.DTOs.Prescriptions;
using Healthcare.Api.Services.Prescriptions;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(
        IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetAll()
    {
        var prescriptions =
            await _prescriptionService.GetAllAsync();

        return Ok(prescriptions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(int id)
    {
        var prescription =
            await _prescriptionService.GetByIdAsync(id);

        if (prescription is null)
            return NotFound();

        return Ok(prescription);
    }

    [HttpPost]
    public async Task<ActionResult<PrescriptionDto>> Create(
        CreatePrescriptionDto dto)
    {
        try
        {
            var prescription =
                await _prescriptionService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = prescription.Id },
                prescription
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PrescriptionDto>> Update(
        int id,
        UpdatePrescriptionDto dto)
    {
        try
        {
            var prescription =
                await _prescriptionService.UpdateAsync(id, dto);

            if (prescription is null)
                return NotFound();

            return Ok(prescription);
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
            await _prescriptionService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}