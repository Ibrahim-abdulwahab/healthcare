using System.Net;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Visits;
using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.DTOs.Users;
using Healthcare.Api.DTOs.Schedules;
using Healthcare.Api.DTOs.PaymentRecords;
using Healthcare.Api.DTOs.AuditLogs;
using Xunit;

namespace Healthcare.Api.Tests;

public class CleanupTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    private async Task<VisitDto> NewVisit(HttpClient staff, DateTime? checkout = null)
    {
        var response = await staff.PostAsJsonAsync("/api/visits", new { appointmentId = 101, patientId = 1, doctorId = 4, locationId = 2, status = "in_progress", checkInTime = DateTime.UtcNow.AddHours(-1), checkOutTime = checkout });
        response.EnsureSuccessStatusCode(); return (await response.Content.ReadFromJsonAsync<VisitDto>())!;
    }

    [Fact]
    public async Task Completion_sets_checkout_once_and_completed_visits_allow_only_timestamp_corrections()
    {
        using var staff = await app.Login(11); using var doctor = await app.Login(4);
        var visit = await NewVisit(staff); var before = DateTime.UtcNow.AddSeconds(-1);
        visit.Status = "completed";
        var result = await doctor.PutAsJsonAsync($"/api/visits/{visit.Id}", visit); result.EnsureSuccessStatusCode();
        var completed = (await result.Content.ReadFromJsonAsync<VisitDto>())!;
        Assert.InRange(completed.CheckOutTime!.Value, before, DateTime.UtcNow.AddSeconds(1));
        var originalCheckout = completed.CheckOutTime;
        completed.CheckOutTime = null;
        result = await doctor.PutAsJsonAsync($"/api/visits/{visit.Id}", completed); result.EnsureSuccessStatusCode();
        Assert.Equal(originalCheckout, (await result.Content.ReadFromJsonAsync<VisitDto>())!.CheckOutTime);
        completed.CheckOutTime = DateTime.UtcNow.AddMinutes(-10);
        completed.CheckOutTime = completed.CheckOutTime.Value.AddTicks(-(completed.CheckOutTime.Value.Ticks % 10)); // PostgreSQL microseconds.
        result = await doctor.PutAsJsonAsync($"/api/visits/{visit.Id}", completed); result.EnsureSuccessStatusCode();
        Assert.Equal(completed.CheckOutTime, (await result.Content.ReadFromJsonAsync<VisitDto>())!.CheckOutTime);
        completed.Status = "in_progress";
        Assert.Equal(HttpStatusCode.Conflict, (await doctor.PutAsJsonAsync($"/api/visits/{visit.Id}", completed)).StatusCode);
    }

    [Fact]
    public async Task Completion_preserves_explicit_or_preexisting_checkout()
    {
        using var staff = await app.Login(11); using var doctor = await app.Login(4);
        var supplied = DateTime.UtcNow.AddMinutes(-20);
        var visit = await NewVisit(staff, supplied);
        // Read the stored timestamp to account for PostgreSQL microsecond precision.
        visit = (await staff.GetFromJsonAsync<VisitDto>($"/api/visits/{visit.Id}"))!;
        supplied = visit.CheckOutTime!.Value;
        visit.Status = "completed"; visit.CheckOutTime = null;
        var update = await doctor.PutAsJsonAsync($"/api/visits/{visit.Id}", visit); update.EnsureSuccessStatusCode();
        Assert.Equal(supplied, (await update.Content.ReadFromJsonAsync<VisitDto>())!.CheckOutTime);
        var second = await NewVisit(staff); second.Status = "completed"; second.CheckOutTime = supplied;
        update = await doctor.PutAsJsonAsync($"/api/visits/{second.Id}", second); update.EnsureSuccessStatusCode();
        Assert.Equal(supplied, (await update.Content.ReadFromJsonAsync<VisitDto>())!.CheckOutTime);
    }

    [Fact]
    public async Task Existing_visit_context_is_immutable_even_without_clinical_children()
    {
        using var staff = await app.Login(11);
        var visit = await NewVisit(staff); visit.AppointmentId = 104; visit.PatientId = 4;
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PutAsJsonAsync($"/api/visits/{visit.Id}", visit)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await staff.PostAsJsonAsync("/api/visits", new { appointmentId = 101, patientId = 4, doctorId = 4, locationId = 2 })).StatusCode);
    }

    [Theory]
    [InlineData(10)][InlineData(11)][InlineData(12)]
    public async Task Any_completed_visit_locks_appointment_status_through_both_write_routes(int user)
    {
        using var client = await app.Login(user);
        var created = await client.PostAsJsonAsync("/api/visits", new { appointmentId = 101, patientId = 1, doctorId = 4, locationId = 2, status = "completed" });
        created.EnsureSuccessStatusCode();
        var appointment = (await client.GetFromJsonAsync<AppointmentDto>("/api/appointments/101"))!;
        Assert.True(appointment.HasCompletedVisits);
        Assert.Equal("booked", appointment.Status); // No backfill, jump, or automatic synchronization.
        foreach (var status in new[] { "arrived", "waiting", "in_consultation", "completed", "cancelled" })
        {
            Assert.Equal(HttpStatusCode.Conflict, (await client.PatchAsJsonAsync("/api/appointments/101/status", new { status })).StatusCode);
            appointment.Status = status;
            Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync("/api/appointments/101", appointment)).StatusCode);
        }
        appointment.Status = "booked"; appointment.Notes = "Allowed booking-note correction";
        (await client.PutAsJsonAsync("/api/appointments/101", appointment)).EnsureSuccessStatusCode();
        Assert.Equal(user == 10 ? HttpStatusCode.Conflict : HttpStatusCode.Forbidden, (await client.DeleteAsync("/api/appointments/101")).StatusCode);
    }

    [Fact]
    public async Task Display_projections_resolve_labels_without_changing_relationships()
    {
        using var admin = await app.Login(10); using var staff = await app.Login(11);
        var user = (await admin.GetFromJsonAsync<UserDto>("/api/users/11"))!;
        Assert.Equal(4, user.StaffDoctorId); Assert.Equal("Four Doctor", user.StaffDoctorName);
        var schedule = (await staff.GetFromJsonAsync<ScheduleDto>("/api/schedules/2"))!;
        Assert.Contains("Four Doctor", schedule.DoctorLocationName); Assert.Contains("Location Two", schedule.DoctorLocationName);
        var payment = (await staff.GetFromJsonAsync<PaymentRecordDto>("/api/paymentrecords/1"))!;
        Assert.Contains("Patient 1 Test", payment.VisitName); Assert.Contains("Four Doctor", payment.VisitName);
        Assert.Equal("Test 11", payment.CollectedByName); Assert.Equal(DateTimeKind.Utc, payment.VisitDatetime.Kind);
        await NewVisit(staff);
        var audit = await admin.GetFromJsonAsync<AuditLogDto[]>("/api/audit-logs");
        Assert.Contains(audit!, a => a.UserId == 11 && a.UserName.Contains("Test 11") && a.UserName.Contains("user11@test.local"));
    }

    [Theory]
    [InlineData(null, null, false)][InlineData(4, 2, false)][InlineData(4, null, true)][InlineData(null, 2, true)]
    public async Task Staff_assignment_validation_is_unchanged(int? doctor, int? organization, bool allowed)
    {
        using var admin = await app.Login(10);
        var response = await admin.PostAsJsonAsync("/api/users", new { name = "Assignment regression", email = $"assignment-{Guid.NewGuid():N}@test.local", password = ApiFixture.Password, role = "staff", staffDoctorId = doctor, staffOrganizationId = organization, isActive = true });
        Assert.Equal(allowed ? HttpStatusCode.Created : HttpStatusCode.BadRequest, response.StatusCode);
    }
}
