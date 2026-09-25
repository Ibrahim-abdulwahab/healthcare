using Healthcare.Api.Data;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();