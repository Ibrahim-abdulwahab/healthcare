using Healthcare.Api.DTOs.Doctors;
using Healthcare.Api.Services.Doctors;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();

        return Ok(doctors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorDto>> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor is null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorDto>> Create(
        CreateDoctorDto dto)
    {
        try
        {
            var doctor = await _doctorService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = doctor.Id },
                doctor
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DoctorDto>> Update(
        int id,
        UpdateDoctorDto dto)
    {
        try
        {
            var doctor = await _doctorService.UpdateAsync(id, dto);

            if (doctor is null)
            {
                return NotFound();
            }

            return Ok(doctor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _doctorService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}