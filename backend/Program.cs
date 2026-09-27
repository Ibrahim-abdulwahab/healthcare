using Healthcare.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Healthcare.Api.Services.Organizations;
using Healthcare.Api.Services.Locations;
using Healthcare.Api.Services.Doctors;
using Healthcare.Api.Services.Users;
using Healthcare.Api.Services.DoctorLocations;
using Healthcare.Api.Services.Schedules;
using Healthcare.Api.Services.ScheduleExceptions;
using Healthcare.Api.Services.Patients;
using Healthcare.Api.Services.VisitTypes;
using Healthcare.Api.Services.Appointments;
using Healthcare.Api.Services.Visits;
using Healthcare.Api.Services.Consultations;
using Healthcare.Api.Services.FollowUps;
using Healthcare.Api.Services.Prescriptions;
using Healthcare.Api.Services.ItemCategories;
using Healthcare.Api.Services.Items;
using Healthcare.Api.Services.PrescriptionItems;
using Healthcare.Api.Services.PaymentRecords;
using Healthcare.Api.Services.LabRequests;
using Healthcare.Api.Services.ImagingRequests;
using Healthcare.Api.Services.Roles;
using Healthcare.Api.Services.Permissions;
using Healthcare.Api.Services.UserRoles;
using Healthcare.Api.Services.Notifications;
using Healthcare.Api.Services.HealthcareFiles;
using Healthcare.Api.Services.AuditLogs;
using Healthcare.Api.Services.SystemSettings;
using Healthcare.Api.Services.CodeLists;
using Healthcare.Api.Configuration;
using Healthcare.Api.Services.Auth;
using Healthcare.Api.Services.Availability;
using Healthcare.Api.Security;
using Healthcare.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// Entity Framework Core + PostgreSQL
builder.Services.AddDbContext<HealthcareDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Register services
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDoctorLocationService, DoctorLocationService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IScheduleExceptionService, ScheduleExceptionService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVisitTypeService, VisitTypeService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IItemCategoryService, ItemCategoryService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
builder.Services.AddScoped<IPaymentRecordService, PaymentRecordService>();
builder.Services.AddScoped<ILabRequestService, LabRequestService>();
builder.Services.AddScoped<IImagingRequestService, ImagingRequestService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHealthcareFileService, HealthcareFileService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<ICodeListService, CodeListService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<Healthcare.Api.Services.Reports.ReportService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestUser>();
builder.Services.AddScoped<ResourceAccess>();
builder.Services.AddScoped<ResourceAccessFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<ResourceAccessFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);
builder.Services.Configure<HealthcareSettings>(
    builder.Configuration.GetSection("Healthcare")
);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
{
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );


    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)
            ),
            ClockSkew = TimeSpan.Zero
        };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdClaim = context.Principal?.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail("Invalid user.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<HealthcareDbContext>();

            var currentUser = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.IsActive)
                .Select(u => new { u.Role, u.StaffDoctorId, u.StaffOrganizationId })
                .SingleOrDefaultAsync();

            if (currentUser is null || currentUser.Role != context.Principal?.FindFirst(ClaimTypes.Role)?.Value)
            {
                context.Fail("User is inactive, no longer exists, or role changed.");
                return;
            }
            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            foreach (var claim in identity.FindAll("staff_doctor_id").Concat(identity.FindAll("staff_organization_id")).ToArray()) identity.RemoveClaim(claim);
            if (currentUser.StaffDoctorId.HasValue) identity.AddClaim(new Claim("staff_doctor_id", currentUser.StaffDoctorId.Value.ToString()));
            if (currentUser.StaffOrganizationId.HasValue) identity.AddClaim(new Claim("staff_organization_id", currentUser.StaffOrganizationId.Value.ToString()));
            if (currentUser.Role == "staff" && currentUser.StaffDoctorId.HasValue == currentUser.StaffOrganizationId.HasValue) context.Fail("Invalid staff assignment.");
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("DoctorOnly", policy =>
        policy.RequireRole("doctor"));

    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("staff"));
});

var app = builder.Build();

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );



if (string.IsNullOrWhiteSpace(jwtSettings.Key) || Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32 ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience) || jwtSettings.ExpirationMinutes <= 0)
    throw new InvalidOperationException("JWT key (at least 32 bytes), issuer, audience and positive expiration are required.");
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    throw new InvalidOperationException("PostgreSQL connection configuration is required.");
TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Healthcare:TimeZone"] ?? "");
app.UseExceptionHandler();
app.UseStatusCodePages(async status =>
{
    var response = status.HttpContext.Response;
    await response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails { Status = response.StatusCode, Title = "Request failed" });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.Run();

public partial class Program { }
