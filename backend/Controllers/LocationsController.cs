using Healthcare.Api.DTOs.Locations;
using Healthcare.Api.Services.Locations;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
    {
        var locations = await _locationService.GetAllAsync();

        return Ok(locations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var location = await _locationService.GetByIdAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        return Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(
        CreateLocationDto dto)
    {
        try
        {
            var location = await _locationService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = location.Id },
                location
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LocationDto>> Update(
        int id,
        UpdateLocationDto dto)
    {
        try
        {
            var location = await _locationService.UpdateAsync(id, dto);

            if (location is null)
            {
                return NotFound();
            }

            return Ok(location);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _locationService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}