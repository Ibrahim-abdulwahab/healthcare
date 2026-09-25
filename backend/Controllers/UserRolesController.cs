using Healthcare.Api.DTOs.UserRoles;
using Healthcare.Api.Services.UserRoles;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/user-roles")]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UserRolesController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetAll()
    {
        var userRoles = await _userRoleService.GetAllAsync();

        return Ok(userRoles);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserRoleDto>> GetById(int id)
    {
        var userRole = await _userRoleService.GetByIdAsync(id);

        if (userRole is null)
            return NotFound();

        return Ok(userRole);
    }

    [HttpPost]
    public async Task<ActionResult<UserRoleDto>> Create(
        CreateUserRoleDto dto)
    {
        try
        {
            var userRole = await _userRoleService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = userRole.Id },
                userRole
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserRoleDto>> Update(
        int id,
        UpdateUserRoleDto dto)
    {
        try
        {
            var userRole = await _userRoleService.UpdateAsync(id, dto);

            if (userRole is null)
                return NotFound();

            return Ok(userRole);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userRoleService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}