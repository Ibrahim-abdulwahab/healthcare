using Healthcare.Api.DTOs.Permissions;
using Healthcare.Api.Services.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAll()
    {
        var permissions = await _permissionService.GetAllAsync();

        return Ok(permissions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PermissionDto>> GetById(int id)
    {
        var permission = await _permissionService.GetByIdAsync(id);

        if (permission is null)
            return NotFound();

        return Ok(permission);
    }

    [HttpPost]
    public async Task<ActionResult<PermissionDto>> Create(
        CreatePermissionDto dto)
    {
        var permission = await _permissionService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = permission.Id },
            permission
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PermissionDto>> Update(
        int id,
        UpdatePermissionDto dto)
    {
        var permission =
            await _permissionService.UpdateAsync(id, dto);

        if (permission is null)
            return NotFound();

        return Ok(permission);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _permissionService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}