using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.AuditLogs;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public AuditLogService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    {
        return await _access.List<AuditLog>()
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User.Name + " (" + a.User.Email + ")",
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AuditLogDto?> GetByIdAsync(int id)
    {
        return await _access.Query<AuditLog>()
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User.Name + " (" + a.User.Email + ")",
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

}
