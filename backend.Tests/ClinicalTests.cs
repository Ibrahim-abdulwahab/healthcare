using System.Net;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Consultations;
using Healthcare.Api.DTOs.PaymentRecords;
using Xunit;

namespace Healthcare.Api.Tests;

public class ClinicalTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Clinical_writes_validate_ownership_and_patient_consistency()
    {
        using var doctor = await app.Login(4);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.PostAsJsonAsync("/api/consultations", new { visitId = 100, notes = "forbidden" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await doctor.PutAsJsonAsync("/api/consultations/101", new { visitId = 101, nextAppointmentId = 102 })).StatusCode);
        var update = await doctor.PutAsJsonAsync("/api/consultations/101", new { visitId = 101, diagnosis = "Updated", createdBy = 10 });
        update.EnsureSuccessStatusCode();
        Assert.Equal(4, (await update.Content.ReadFromJsonAsync<ConsultationDto>())!.CreatedBy);
        Assert.Equal(HttpStatusCode.BadRequest, (await doctor.PostAsJsonAsync("/api/prescriptions", new { consultationId = 101, patientId = 2, doctorId = 4, prescriptionDate = "2030-01-07" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.DeleteAsync("/api/consultations/101")).StatusCode);
    }

    [Fact]
    public async Task Payments_validate_positive_amount_scope_and_immutable_collector()
    {
        using var staff = await app.Login(11);
        var payload = new { visitId = 101, amount = 50, paymentMethod = "cash", paidTo = "doctor", paymentStatus = "partial", collectedBy = 10 };
        var create = await staff.PostAsJsonAsync("/api/paymentrecords", payload);
        create.EnsureSuccessStatusCode();
        var payment = (await create.Content.ReadFromJsonAsync<PaymentRecordDto>())!;
        Assert.Equal(11, payment.CollectedBy);
        using var admin = await app.Login(10);
        var update = await admin.PutAsJsonAsync($"/api/paymentrecords/{payment.Id}", payload);
        update.EnsureSuccessStatusCode();
        Assert.Equal(11, (await update.Content.ReadFromJsonAsync<PaymentRecordDto>())!.CollectedBy);
        Assert.Equal(HttpStatusCode.BadRequest, (await staff.PostAsJsonAsync("/api/paymentrecords", new { visitId = 101, amount = 0, paymentMethod = "cash", paidTo = "doctor", paymentStatus = "paid" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/paymentrecords", new { visitId = 100, amount = 25, paymentMethod = "cash", paidTo = "doctor", paymentStatus = "paid" })).StatusCode);
    }

    [Fact]
    public async Task Files_inherit_parent_write_permission_and_reject_unknown_tables()
    {
        using var doctor = await app.Login(4);
        using var staff = await app.Login(11);
        var file = new { tableName = "consultations", recordId = 101, fileName = "reference", fileUrl = "https://example.invalid/file.pdf", uploadedBy = 10 };
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/files", file)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await doctor.PostAsJsonAsync("/api/files", file)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await doctor.PostAsJsonAsync("/api/files", new { tableName = "users", recordId = 10, fileName = "bad", fileUrl = "https://example.invalid" })).StatusCode);
    }

    [Fact]
    public async Task Mark_read_cannot_change_recipient_and_audit_has_no_write_API()
    {
        using var doctor = await app.Login(4);
        using var other = await app.Login(1);
        using var admin = await app.Login(10);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PatchAsJsonAsync("/api/notifications/1/read", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await doctor.PatchAsJsonAsync("/api/notifications/1/read", new { userId = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await admin.PostAsJsonAsync("/api/audit-logs", new { userId = 10, action = "forged", tableName = "patients", recordId = 1 })).StatusCode);
        var logs = await admin.GetFromJsonAsync<Healthcare.Api.DTOs.AuditLogs.AuditLogDto[]>("/api/audit-logs");
        Assert.Contains(logs!, l => l.UserId == 4 && l.TableName == "notifications");
    }
}
