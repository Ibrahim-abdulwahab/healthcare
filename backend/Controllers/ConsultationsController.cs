using Healthcare.Api.DTOs.Consultations;
using Healthcare.Api.Services.Consultations;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(
        IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetAll()
    {
        var consultations = await _consultationService.GetAllAsync();

        return Ok(consultations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConsultationDto>> GetById(int id)
    {
        var consultation = await _consultationService.GetByIdAsync(id);

        if (consultation is null)
            return NotFound();

        return Ok(consultation);
    }

    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> Create(
        CreateConsultationDto dto)
    {
        try
        {
            var consultation =
                await _consultationService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = consultation.Id },
                consultation
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ConsultationDto>> Update(
        int id,
        UpdateConsultationDto dto)
    {
        try
        {
            var consultation =
                await _consultationService.UpdateAsync(id, dto);

            if (consultation is null)
                return NotFound();

            return Ok(consultation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _consultationService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}