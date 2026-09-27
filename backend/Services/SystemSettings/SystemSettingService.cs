using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.SystemSettings;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.SystemSettings;

public class SystemSettingService : ISystemSettingService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public SystemSettingService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<SystemSettingDto>> GetAllAsync()
    {
        return await _access.List<SystemSetting>()
            .Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<SystemSettingDto?> GetByIdAsync(int id)
    {
        return await _access.Query<SystemSetting>()
            .Where(s => s.Id == id)
            .Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SystemSettingDto> CreateAsync(
        CreateSystemSettingDto dto)
    {
        var now = DateTime.UtcNow;

        var systemSetting = new SystemSetting
        {
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.SystemSettings.Add(systemSetting);
        await _context.SaveChangesAsync();

        return ToDto(systemSetting);
    }

    public async Task<SystemSettingDto?> UpdateAsync(
        int id,
        UpdateSystemSettingDto dto)
    {
        var systemSetting =
            await _context.SystemSettings.FindAsync(id);

        if (systemSetting is null)
            return null;

        systemSetting.Key = dto.Key;
        systemSetting.Value = dto.Value;
        systemSetting.Description = dto.Description;
        systemSetting.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(systemSetting);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var systemSetting =
            await _context.SystemSettings.FindAsync(id);

        if (systemSetting is null)
            return false;

        _context.SystemSettings.Remove(systemSetting);
        await _context.SaveChangesAsync();

        return true;
    }

    private static SystemSettingDto ToDto(
        SystemSetting systemSetting)
    {
        return new SystemSettingDto
        {
            Id = systemSetting.Id,
            Key = systemSetting.Key,
            Value = systemSetting.Value,
            Description = systemSetting.Description,
            CreatedAt = systemSetting.CreatedAt,
            UpdatedAt = systemSetting.UpdatedAt
        };
    }
}