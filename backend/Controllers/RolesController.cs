using Healthcare.Api.DTOs.Roles;
using Healthcare.Api.Services.Roles;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _roleService.GetAllAsync();

        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);

        if (role is null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create(
        CreateRoleDto dto)
    {
        var role = await _roleService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = role.Id },
            role
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleDto>> Update(
        int id,
        UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateAsync(id, dto);

        if (role is null)
            return NotFound();

        return Ok(role);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _roleService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}