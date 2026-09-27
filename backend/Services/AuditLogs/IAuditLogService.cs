using Healthcare.Api.DTOs.AuditLogs;

namespace Healthcare.Api.Services.AuditLogs;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync();

    Task<AuditLogDto?> GetByIdAsync(int id);

}
