using Healthcare.Api.DTOs.Organizations;
using Healthcare.Api.Services.Organizations;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrganizationDto>>> GetAll()
    {
        var organizations = await _organizationService.GetAllAsync();

        return Ok(organizations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganizationDto>> GetById(int id)
    {
        var organization = await _organizationService.GetByIdAsync(id);

        if (organization is null)
        {
            return NotFound();
        }

        return Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> Create(
        CreateOrganizationDto dto)
    {
        var organization = await _organizationService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            organization
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrganizationDto>> Update(
        int id,
        UpdateOrganizationDto dto)
    {
        var organization = await _organizationService.UpdateAsync(id, dto);

        if (organization is null)
        {
            return NotFound();
        }

        return Ok(organization);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _organizationService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}