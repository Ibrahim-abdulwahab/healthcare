using Healthcare.Api.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService reports) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int? doctorId, [FromQuery] int? locationId, [FromQuery] string? status,
        [FromQuery] string? paymentMethod, [FromQuery] string? paidTo)
        => Ok(await reports.GetAsync(from, to, doctorId, locationId, status, paymentMethod, paidTo));
}
