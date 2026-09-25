using Healthcare.Api.DTOs.ImagingRequests;
using Healthcare.Api.Services.ImagingRequests;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagingRequestsController : ControllerBase
{
    private readonly IImagingRequestService _imagingRequestService;

    public ImagingRequestsController(
        IImagingRequestService imagingRequestService)
    {
        _imagingRequestService = imagingRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ImagingRequestDto>>> GetAll()
    {
        var imagingRequests =
            await _imagingRequestService.GetAllAsync();

        return Ok(imagingRequests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ImagingRequestDto>> GetById(int id)
    {
        var imagingRequest =
            await _imagingRequestService.GetByIdAsync(id);

        if (imagingRequest is null)
            return NotFound();

        return Ok(imagingRequest);
    }

    [HttpPost]
    public async Task<ActionResult<ImagingRequestDto>> Create(
        CreateImagingRequestDto dto)
    {
        try
        {
            var imagingRequest =
                await _imagingRequestService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = imagingRequest.Id },
                imagingRequest
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ImagingRequestDto>> Update(
        int id,
        UpdateImagingRequestDto dto)
    {
        try
        {
            var imagingRequest =
                await _imagingRequestService.UpdateAsync(id, dto);

            if (imagingRequest is null)
                return NotFound();

            return Ok(imagingRequest);
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
            await _imagingRequestService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}