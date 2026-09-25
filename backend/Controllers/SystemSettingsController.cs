using Healthcare.Api.DTOs.SystemSettings;
using Healthcare.Api.Services.SystemSettings;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/system-settings")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;

    public SystemSettingsController(
        ISystemSettingService systemSettingService)
    {
        _systemSettingService = systemSettingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SystemSettingDto>>> GetAll()
    {
        var settings = await _systemSettingService.GetAllAsync();

        return Ok(settings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SystemSettingDto>> GetById(int id)
    {
        var setting = await _systemSettingService.GetByIdAsync(id);

        if (setting is null)
            return NotFound();

        return Ok(setting);
    }

    [HttpPost]
    public async Task<ActionResult<SystemSettingDto>> Create(
        CreateSystemSettingDto dto)
    {
        var setting = await _systemSettingService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = setting.Id },
            setting
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SystemSettingDto>> Update(
        int id,
        UpdateSystemSettingDto dto)
    {
        var setting = await _systemSettingService.UpdateAsync(id, dto);

        if (setting is null)
            return NotFound();

        return Ok(setting);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _systemSettingService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}