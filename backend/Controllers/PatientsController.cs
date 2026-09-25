using Healthcare.Api.DTOs.Patients;
using Healthcare.Api.Services.Patients;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);

        if (patient is null)
        {
            return NotFound();
        }

        return Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(
        CreatePatientDto dto)
    {
        try
        {
            var patient = await _patientService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = patient.Id },
                patient
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDto>> Update(
        int id,
        UpdatePatientDto dto)
    {
        try
        {
            var patient = await _patientService.UpdateAsync(id, dto);

            if (patient is null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _patientService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}