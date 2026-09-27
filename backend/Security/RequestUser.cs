using System.Security.Claims;

namespace Healthcare.Api.Security;

public sealed class RequestUser(IHttpContextAccessor http)
{
    private ClaimsPrincipal Principal => http.HttpContext?.User ?? new ClaimsPrincipal();
    public int Id => int.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    public string Role => Principal.FindFirstValue(ClaimTypes.Role) ?? "";
    public bool IsAdmin => Role == "admin";
    public string? ExpectedUpdatedAt => http.HttpContext?.Request.Headers["X-Expected-Updated-At"].FirstOrDefault();
    public int? DoctorId => Role == "doctor" ? Id : ClaimInt("staff_doctor_id");
    public int? OrganizationId => Role == "staff" ? ClaimInt("staff_organization_id") : null;
    private int? ClaimInt(string type) => int.TryParse(Principal.FindFirstValue(type), out var id) ? id : null;
}
