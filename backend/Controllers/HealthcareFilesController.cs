using Healthcare.Api.DTOs.HealthcareFiles;
using Healthcare.Api.Services.HealthcareFiles;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/files")]
public class HealthcareFilesController : ControllerBase
{
    private readonly IHealthcareFileService _healthcareFileService;

    public HealthcareFilesController(
        IHealthcareFileService healthcareFileService)
    {
        _healthcareFileService = healthcareFileService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HealthcareFileDto>>> GetAll()
    {
        var files = await _healthcareFileService.GetAllAsync();

        return Ok(files);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HealthcareFileDto>> GetById(int id)
    {
        var file = await _healthcareFileService.GetByIdAsync(id);

        if (file is null)
            return NotFound();

        return Ok(file);
    }

    [HttpPost]
    public async Task<ActionResult<HealthcareFileDto>> Create(
        CreateHealthcareFileDto dto)
    {
        try
        {
            var file = await _healthcareFileService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = file.Id },
                file
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HealthcareFileDto>> Update(
        int id,
        UpdateHealthcareFileDto dto)
    {
        try
        {
            var file = await _healthcareFileService.UpdateAsync(id, dto);

            if (file is null)
                return NotFound();

            return Ok(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _healthcareFileService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}