using Healthcare.Api.Data;
using Healthcare.Api.Infrastructure;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Healthcare.Api.Security;

public sealed class ResourceAccessFilter(ResourceAccess access, RequestUser user, HealthcareDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resource = context.RouteData.Values["controller"]?.ToString() ?? "";
        if (resource is "Auth" or "Availability") { await next(); return; }
        var method = context.HttpContext.Request.Method;
        if (!AccessPolicy.Allows(user.Role, resource, method)) { context.Result = new ForbidResult(); return; }
        IDbContextTransaction? transaction = null;
        try
        {
            if (method != "GET")
            {
                transaction = await db.Database.BeginTransactionAsync(context.HttpContext.RequestAborted);
                // ponytail: serialize API mutations across instances; use ordered per-doctor/resource locks if write throughput warrants it.
                await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(727410, 1)", context.HttpContext.RequestAborted);
            }
            var id = context.ActionArguments.TryGetValue("id", out var value) ? Convert.ToInt32(value) : 0;
            if (id > 0 && !await access.CanRead(resource, id)) throw new ApiException(404, "Record not found in your scope.");
            var dto = context.ActionArguments.Values.FirstOrDefault(v => v?.GetType().Namespace?.StartsWith("Healthcare.Api.DTOs") == true);
            if (method != "GET" && dto != null) await CheckTarget(resource, dto, id);
            if (resource == "HealthcareFiles" && id > 0 && method != "GET")
            {
                var file = await db.Files.AsNoTracking().SingleAsync(f => f.Id == id);
                await CheckFile(file.TableName, file.RecordId, true);
            }
            var result = await next();
            var status = (result.Result as ObjectResult)?.StatusCode ?? (result.Result as StatusCodeResult)?.StatusCode ?? 200;
            if (result.Exception == null && status < 400 && transaction != null) await transaction.CommitAsync();
            if (result.Result is BadRequestObjectResult bad && bad.Value is not ProblemDetails)
            {
                var message = bad.Value as string ?? bad.Value?.GetType().GetProperty("message")?.GetValue(bad.Value)?.ToString();
                result.Result = new BadRequestObjectResult(new ProblemDetails { Status = 400, Title = "Validation failed", Detail = message });
            }
        }
        finally { if (transaction != null) await transaction.DisposeAsync(); }
    }

    private async Task CheckTarget(string resource, object dto, int recordId)
    {
        int Id(string field) => Convert.ToInt32(dto.GetType().GetProperty(field)?.GetValue(dto) ?? 0);
        async Task Require(string parent, int id)
        {
            if (!await access.CanRead(parent, id)) throw new ApiException(403, "The target resource is outside your scope.");
        }
        switch (resource)
        {
            case "Appointments":
                if (dto is Healthcare.Api.DTOs.Appointments.ChangeAppointmentStatusDto) break;
                var doctorId = Id("DoctorId");
                await Require("Doctors", doctorId);
                var isBooking = dto is Healthcare.Api.DTOs.Appointments.CreateAppointmentDto or Healthcare.Api.DTOs.Appointments.RescheduleAppointmentDto;
                if (dto is Healthcare.Api.DTOs.Appointments.UpdateAppointmentDto update)
                    isBooking = await db.Appointments.AnyAsync(a => a.Id == recordId &&
                        (a.DoctorId != update.DoctorId || a.LocationId != update.LocationId || a.VisitTypeId != update.VisitTypeId || a.StartDatetime != update.StartDatetime));
                if (isBooking) await Require("Locations", Id("LocationId"));
                break;
            case "Visits": await Require("Appointments", Id("AppointmentId")); await Require("Doctors", Id("DoctorId")); break;
            case "Consultations": case "LabRequests": case "ImagingRequests": case "PaymentRecords": await Require("Visits", Id("VisitId")); break;
            case "Prescriptions":
                await Require("Consultations", Id("ConsultationId")); await Require("Doctors", Id("DoctorId")); break;
            case "PrescriptionItems": await Require("Prescriptions", Id("PrescriptionId")); break;
            case "FollowUps":
                await Require("Doctors", Id("DoctorId")); await Require("Patients", Id("PatientId"));
                if (Id("AppointmentId") > 0) await Require("Appointments", Id("AppointmentId")); break;
            case "Schedules": case "ScheduleExceptions": await Require("DoctorLocations", Id("DoctorLocationId")); break;
            case "HealthcareFiles":
                await CheckFile(dto.GetType().GetProperty("TableName")?.GetValue(dto)?.ToString() ?? "", Id("RecordId"), true); break;
        }
    }

    private async Task CheckFile(string table, int id, bool write)
    {
        var parent = ResourceAccess.FileParent(table);
        if (!AccessPolicy.Allows(user.Role, parent, write ? "PUT" : "GET") || !await access.CanRead(parent, id))
            throw new ApiException(403, "You cannot access this file's parent resource.");
    }
}
