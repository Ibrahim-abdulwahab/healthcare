using Healthcare.Api.DTOs.DoctorLocations;
using Healthcare.Api.Services.DoctorLocations;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/doctor-locations")]
public class DoctorLocationsController : ControllerBase
{
    private readonly IDoctorLocationService _doctorLocationService;

    public DoctorLocationsController(
        IDoctorLocationService doctorLocationService)
    {
        _doctorLocationService = doctorLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DoctorLocationDto>>> GetAll()
    {
        var doctorLocations =
            await _doctorLocationService.GetAllAsync();

        return Ok(doctorLocations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorLocationDto>> GetById(int id)
    {
        var doctorLocation =
            await _doctorLocationService.GetByIdAsync(id);

        if (doctorLocation is null)
        {
            return NotFound();
        }

        return Ok(doctorLocation);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorLocationDto>> Create(
        CreateDoctorLocationDto dto)
    {
        try
        {
            var doctorLocation =
                await _doctorLocationService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = doctorLocation.Id },
                doctorLocation
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DoctorLocationDto>> Update(
        int id,
        UpdateDoctorLocationDto dto)
    {
        try
        {
            var doctorLocation =
                await _doctorLocationService.UpdateAsync(id, dto);

            if (doctorLocation is null)
            {
                return NotFound();
            }

            return Ok(doctorLocation);
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
            await _doctorLocationService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}