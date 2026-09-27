using System.Net;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Patients;
using Healthcare.Api.DTOs.Consultations;
using Xunit;

namespace Healthcare.Api.Tests;

public class AuthorizationTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Theory]
    [InlineData("patients")][InlineData("visits")][InlineData("consultations")][InlineData("followups")]
    [InlineData("prescriptions")][InlineData("prescriptionitems")][InlineData("paymentrecords")][InlineData("labrequests")]
    [InlineData("imagingrequests")][InlineData("files")][InlineData("notifications")][InlineData("audit-logs")]
    [InlineData("users")][InlineData("roles")][InlineData("permissions")][InlineData("user-roles")]
    [InlineData("system-settings")][InlineData("code-lists")][InlineData("schedules")][InlineData("schedule-exceptions")]
    public async Task Anonymous_cannot_read_protected_resources(string route)
    {
        using var client = app.CreateClient(new() { BaseAddress = new("https://localhost") });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/" + route)).StatusCode);
    }

    [Theory]
    [InlineData(4)][InlineData(11)][InlineData(12)]
    public async Task Doctor_and_both_secretary_scopes_filter_patients_and_clinical_records(int user)
    {
        using var client = await app.Login(user);
        var patients = await client.GetFromJsonAsync<PatientDto[]>("/api/patients");
        Assert.Contains(patients!, p => p.Id == 1); // shared patient
        Assert.Contains(patients!, p => p.Id == 4); // historical visit, cancelled booking
        Assert.DoesNotContain(patients!, p => p.Id is 2 or 3 or 5);
        var notes = await client.GetFromJsonAsync<ConsultationDto[]>("/api/consultations");
        Assert.Contains(notes!, n => n.Id == 101);
        Assert.DoesNotContain(notes!, n => n.Id == 100);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/consultations/100")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/files/1")).StatusCode);
    }

    [Theory]
    [InlineData(1)][InlineData(13)]
    public async Task Other_doctor_and_secretary_cannot_read_clinical_attachment(int user)
    {
        using var client = await app.Login(user);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/files/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/consultations/101")).StatusCode);
    }

    [Fact]
    public async Task Doctor_cannot_edit_demographics_or_appointments_and_staff_cannot_author_consultations()
    {
        using var doctor = await app.Login(4);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.PutAsJsonAsync("/api/patients/1", new { mrn = "TEST-1", firstName = "Changed", lastName = "Test" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = app.Start })).StatusCode);
        using var staff = await app.Login(11);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/consultations", new { visitId = 101 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/prescriptions", new { consultationId = 101, doctorId = 4, patientId = 1, prescriptionDate = "2030-01-07" })).StatusCode);
    }

    [Theory]
    [InlineData(11)][InlineData(12)]
    public async Task Staff_cannot_book_for_doctor_outside_assignment(int user)
    {
        using var staff = await app.Login(user);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 1, locationId = 1, visitTypeId = 1, startDatetime = app.Start, endDatetime = app.Start.AddMinutes(30) })).StatusCode);
    }

    [Fact]
    public async Task Administrative_resources_and_notifications_have_distinct_permissions()
    {
        using var doctor = await app.Login(4);
        using var other = await app.Login(1);
        using var admin = await app.Login(10);
        foreach (var route in new[] { "roles", "permissions", "user-roles", "system-settings", "audit-logs" })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await doctor.GetAsync("/api/" + route)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/" + route)).StatusCode);
        }
        Assert.Equal(HttpStatusCode.OK, (await doctor.GetAsync("/api/notifications/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync("/api/notifications/1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await doctor.GetAsync("/api/doctor-locations")).StatusCode);
    }
}
