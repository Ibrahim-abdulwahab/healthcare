using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Organizations;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Organizations;

public class OrganizationService : IOrganizationService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public OrganizationService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<List<OrganizationDto>> GetAllAsync()
    {
        return await _access.List<Organization>()
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Type = o.Type,
                Address = o.Address,
                Phone = o.Phone,
                Email = o.Email,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<OrganizationDto?> GetByIdAsync(int id)
    {
        return await _access.Query<Organization>()
            .Where(o => o.Id == id)
            .Select(o => new OrganizationDto
            {
                Id = o.Id,
                Name = o.Name,
                Type = o.Type,
                Address = o.Address,
                Phone = o.Phone,
                Email = o.Email,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto)
    {
        var organization = new Organization
        {
            Name = dto.Name,
            Type = dto.Type,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);

        await _context.SaveChangesAsync();

        return new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Type = organization.Type,
            Address = organization.Address,
            Phone = organization.Phone,
            Email = organization.Email,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }

    public async Task<OrganizationDto?> UpdateAsync(
    int id,
    UpdateOrganizationDto dto)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization is null)
        {
            return null;
        }

        organization.Name = dto.Name;
        organization.Type = dto.Type;
        organization.Address = dto.Address;
        organization.Phone = dto.Phone;
        organization.Email = dto.Email;
        organization.IsActive = dto.IsActive;
        organization.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Type = organization.Type,
            Address = organization.Address,
            Phone = organization.Phone,
            Email = organization.Email,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization is null)
        {
            return false;
        }

        _context.Organizations.Remove(organization);

        await _context.SaveChangesAsync();

        return true;
    }
}