using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Auth;
using Healthcare.Api.DTOs.Users;
using Healthcare.Api.DTOs.Patients;
using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.DTOs.HealthcareFiles;
using Xunit;

namespace Healthcare.Api.Tests;

public class HardeningTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Database_role_changes_invalidate_existing_privileged_token()
    {
        using var admin = await app.Login(10);
        var created = await admin.PostAsJsonAsync("/api/users", new { name = "Temporary", email = "temporary@test.local", password = ApiFixture.Password, role = "admin", isActive = true });
        created.EnsureSuccessStatusCode(); var user = (await created.Content.ReadFromJsonAsync<UserDto>())!;
        using var temporary = app.CreateClient(new() { BaseAddress = new("https://localhost") });
        var login = await temporary.PostAsJsonAsync("/api/auth/login", new { email = user.Email, password = ApiFixture.Password });
        login.EnsureSuccessStatusCode(); var token = (await login.Content.ReadFromJsonAsync<LoginResponseDto>())!.Token;
        temporary.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.OK, (await temporary.GetAsync("/api/users")).StatusCode);
        (await admin.PutAsJsonAsync($"/api/users/{user.Id}", new { name = user.Name, email = user.Email, role = "staff", staffDoctorId = 4, isActive = true })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await temporary.GetAsync("/api/users")).StatusCode);
    }

    [Theory]
    [InlineData(1)][InlineData(4)][InlineData(11)][InlineData(12)][InlineData(13)]
    public async Task Authenticated_lookup_reads_include_individual_records(int user)
    {
        using var client = await app.Login(user);
        foreach (var path in new[] { "items/1", "itemcategories/1", "visit-types/1" })
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/" + path)).StatusCode);
    }

    [Fact]
    public async Task Modification_precondition_prevents_silent_demographic_overwrite()
    {
        using var admin = await app.Login(10);
        var patient = (await admin.GetFromJsonAsync<PatientDto>("/api/patients/2"))!;
        admin.DefaultRequestHeaders.Add("X-Expected-Updated-At", patient.UpdatedAt.ToString("O"));
        var dto = new { mrn = patient.Mrn, firstName = "First edit", lastName = patient.LastName };
        (await admin.PutAsJsonAsync("/api/patients/2", dto)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PutAsJsonAsync("/api/patients/2", new { mrn = patient.Mrn, firstName = "Stale edit", lastName = patient.LastName })).StatusCode);
        Assert.Equal("First edit", (await admin.GetFromJsonAsync<PatientDto>("/api/patients/2"))!.FirstName);
    }

    [Fact]
    public async Task Complete_lifecycle_cannot_reopen_and_preserves_actor()
    {
        using var staff = await app.Login(11);
        var create = await staff.PostAsJsonAsync("/api/appointments", new { doctorId = 4, locationId = 2, patientId = 1, visitTypeId = 1, startDatetime = app.Start.AddDays(28) });
        create.EnsureSuccessStatusCode(); var appointment = (await create.Content.ReadFromJsonAsync<AppointmentDto>())!;
        foreach (var status in new[] { "arrived", "waiting", "in_consultation", "completed" })
        {
            var update = await staff.PatchAsJsonAsync($"/api/appointments/{appointment.Id}/status", new { status });
            update.EnsureSuccessStatusCode(); Assert.Equal(status, (await update.Content.ReadFromJsonAsync<AppointmentDto>())!.Status);
        }
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PatchAsJsonAsync($"/api/appointments/{appointment.Id}/status", new { status = "cancelled" })).StatusCode);
        Assert.Equal(11, (await staff.GetFromJsonAsync<AppointmentDto>($"/api/appointments/{appointment.Id}"))!.CreatedBy);
    }

    [Fact]
    public async Task Schedule_ownership_duplicate_mapping_and_file_attribution_are_enforced()
    {
        using var doctor = await app.Login(4);
        using var admin = await app.Login(10);
        var otherSchedule = new { doctorLocationId = 1, dayOfWeek = 1, startTime = "09:00", endTime = "13:00", slotDuration = 20, isActive = true };
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.PostAsJsonAsync("/api/schedules", otherSchedule)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await doctor.DeleteAsync("/api/schedules/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync("/api/doctor-locations", new { doctorId = 4, locationId = 2, isActive = true })).StatusCode);
        var file = await doctor.PostAsJsonAsync("/api/files", new { tableName = "consultations", recordId = 101, fileName = "Secure reference", fileUrl = "https://example.invalid/reference", uploadedBy = 10 });
        file.EnsureSuccessStatusCode(); Assert.Equal(4, (await file.Content.ReadFromJsonAsync<HealthcareFileDto>())!.UploadedBy);
        Assert.Equal(HttpStatusCode.BadRequest, (await doctor.PostAsJsonAsync("/api/files", new { tableName = "consultations", recordId = 101, fileName = "Unsafe link", fileUrl = "javascript:alert(1)" })).StatusCode);
    }
}
