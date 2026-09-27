using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.HealthcareFiles;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.HealthcareFiles;

public class HealthcareFileService : IHealthcareFileService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public HealthcareFileService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<HealthcareFileDto>> GetAllAsync()
    {
        return await _access.List<HealthcareFile>()
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
        return await _access.Query<HealthcareFile>()
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
        ResourceAccess.FileParent(dto.TableName);
        if (!Uri.TryCreate(dto.FileUrl, UriKind.Absolute, out var url) || url.Scheme != "https")
            throw new ArgumentException("File URL must be an absolute HTTPS URL. Binary upload is not supported.");
        await ValidateUploadedByAsync(_context.ActorId);

        var healthcareFile = new HealthcareFile
        {
            TableName = dto.TableName,
            RecordId = dto.RecordId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            UploadedBy = _context.ActorId,
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

        ResourceAccess.FileParent(dto.TableName);
        if (!Uri.TryCreate(dto.FileUrl, UriKind.Absolute, out var url) || url.Scheme != "https")
            throw new ArgumentException("File URL must be an absolute HTTPS URL. Binary upload is not supported.");
        await ValidateUploadedByAsync(healthcareFile.UploadedBy);

        healthcareFile.TableName = dto.TableName;
        healthcareFile.RecordId = dto.RecordId;
        healthcareFile.FileName = dto.FileName;
        healthcareFile.FileUrl = dto.FileUrl;

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