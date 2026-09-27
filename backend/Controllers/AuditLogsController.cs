using Healthcare.Api.DTOs.AuditLogs;
using Healthcare.Api.Services.AuditLogs;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetAll()
    {
        var auditLogs = await _auditLogService.GetAllAsync();

        return Ok(auditLogs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuditLogDto>> GetById(int id)
    {
        var auditLog = await _auditLogService.GetByIdAsync(id);

        if (auditLog is null)
            return NotFound();

        return Ok(auditLog);
    }

}
