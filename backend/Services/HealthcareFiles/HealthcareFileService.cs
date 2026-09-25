using Healthcare.Api.Data;
using Healthcare.Api.DTOs.HealthcareFiles;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.HealthcareFiles;

public class HealthcareFileService : IHealthcareFileService
{
    private readonly HealthcareDbContext _context;

    public HealthcareFileService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HealthcareFileDto>> GetAllAsync()
    {
        return await _context.Files
            .AsNoTracking()
            .Select(f => new HealthcareFileDto
            {
                Id = f.Id,
                TableName = f.TableName,
                RecordId = f.RecordId,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                UploadedBy = f.UploadedBy,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<HealthcareFileDto?> GetByIdAsync(int id)
    {
        return await _context.Files
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new HealthcareFileDto
            {
                Id = f.Id,
                TableName = f.TableName,
                RecordId = f.RecordId,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                UploadedBy = f.UploadedBy,
                CreatedAt = f.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<HealthcareFileDto> CreateAsync(
        CreateHealthcareFileDto dto)
    {
        await ValidateUploadedByAsync(dto.UploadedBy);

        var healthcareFile = new HealthcareFile
        {
            TableName = dto.TableName,
            RecordId = dto.RecordId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            UploadedBy = dto.UploadedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Files.Add(healthcareFile);
        await _context.SaveChangesAsync();

        return ToDto(healthcareFile);
    }

    public async Task<HealthcareFileDto?> UpdateAsync(
        int id,
        UpdateHealthcareFileDto dto)
    {
        var healthcareFile =
            await _context.Files.FindAsync(id);

        if (healthcareFile is null)
            return null;

        await ValidateUploadedByAsync(dto.UploadedBy);

        healthcareFile.TableName = dto.TableName;
        healthcareFile.RecordId = dto.RecordId;
        healthcareFile.FileName = dto.FileName;
        healthcareFile.FileUrl = dto.FileUrl;
        healthcareFile.UploadedBy = dto.UploadedBy;

        await _context.SaveChangesAsync();

        return ToDto(healthcareFile);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var healthcareFile =
            await _context.Files.FindAsync(id);

        if (healthcareFile is null)
            return false;

        _context.Files.Remove(healthcareFile);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateUploadedByAsync(int userId)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new ArgumentException(
                $"User with ID {userId} does not exist."
            );
        }
    }

    private static HealthcareFileDto ToDto(
        HealthcareFile healthcareFile)
    {
        return new HealthcareFileDto
        {
            Id = healthcareFile.Id,
            TableName = healthcareFile.TableName,
            RecordId = healthcareFile.RecordId,
            FileName = healthcareFile.FileName,
            FileUrl = healthcareFile.FileUrl,
            UploadedBy = healthcareFile.UploadedBy,
            CreatedAt = healthcareFile.CreatedAt
        };
    }
}