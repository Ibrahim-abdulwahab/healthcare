using Healthcare.Api.Data;
using Healthcare.Api.DTOs.AuditLogs;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly HealthcareDbContext _context;

    public AuditLogService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AuditLogDto?> GetByIdAsync(int id)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AuditLogDto> CreateAsync(
        CreateAuditLogDto dto)
    {
        await ValidateUserAsync(dto.UserId);

        var auditLog = new AuditLog
        {
            UserId = dto.UserId,
            Action = dto.Action,
            TableName = dto.TableName,
            RecordId = dto.RecordId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return ToDto(auditLog);
    }

    public async Task<AuditLogDto?> UpdateAsync(
        int id,
        UpdateAuditLogDto dto)
    {
        var auditLog = await _context.AuditLogs.FindAsync(id);

        if (auditLog is null)
            return null;

        await ValidateUserAsync(dto.UserId);

        auditLog.UserId = dto.UserId;
        auditLog.Action = dto.Action;
        auditLog.TableName = dto.TableName;
        auditLog.RecordId = dto.RecordId;

        await _context.SaveChangesAsync();

        return ToDto(auditLog);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var auditLog = await _context.AuditLogs.FindAsync(id);

        if (auditLog is null)
            return false;

        _context.AuditLogs.Remove(auditLog);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateUserAsync(int userId)
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

    private static AuditLogDto ToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            TableName = auditLog.TableName,
            RecordId = auditLog.RecordId,
            CreatedAt = auditLog.CreatedAt
        };
    }
}