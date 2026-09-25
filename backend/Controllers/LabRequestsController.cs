using Healthcare.Api.DTOs.LabRequests;
using Healthcare.Api.Services.LabRequests;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabRequestsController : ControllerBase
{
    private readonly ILabRequestService _labRequestService;

    public LabRequestsController(ILabRequestService labRequestService)
    {
        _labRequestService = labRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabRequestDto>>> GetAll()
    {
        var labRequests = await _labRequestService.GetAllAsync();

        return Ok(labRequests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LabRequestDto>> GetById(int id)
    {
        var labRequest = await _labRequestService.GetByIdAsync(id);

        if (labRequest is null)
            return NotFound();

        return Ok(labRequest);
    }

    [HttpPost]
    public async Task<ActionResult<LabRequestDto>> Create(
        CreateLabRequestDto dto)
    {
        try
        {
            var labRequest = await _labRequestService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = labRequest.Id },
                labRequest
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LabRequestDto>> Update(
        int id,
        UpdateLabRequestDto dto)
    {
        try
        {
            var labRequest =
                await _labRequestService.UpdateAsync(id, dto);

            if (labRequest is null)
                return NotFound();

            return Ok(labRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _labRequestService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}