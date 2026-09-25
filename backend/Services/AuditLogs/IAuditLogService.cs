using Healthcare.Api.DTOs.AuditLogs;

namespace Healthcare.Api.Services.AuditLogs;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync();

    Task<AuditLogDto?> GetByIdAsync(int id);

    Task<AuditLogDto> CreateAsync(
        CreateAuditLogDto dto
    );

    Task<AuditLogDto?> UpdateAsync(
        int id,
        UpdateAuditLogDto dto
    );

    Task<bool> DeleteAsync(int id);
}