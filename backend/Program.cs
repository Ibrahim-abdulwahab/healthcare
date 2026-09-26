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

builder.Services.AddControllers();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);
builder.Services.Configure<HealthcareSettings>(
    builder.Configuration.GetSection("Healthcare")
);

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
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

            var isActive = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId && u.IsActive);

            if (!isActive)
            {
                context.Fail("User is inactive or no longer exists.");
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("DoctorOnly", policy =>
        policy.RequireRole("doctor"));

    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("staff"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();