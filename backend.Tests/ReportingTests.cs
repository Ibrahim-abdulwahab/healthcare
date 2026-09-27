using System.Net;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Reports;
using Xunit;

namespace Healthcare.Api.Tests;

public class ReportingTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Theory]
    [InlineData(4)][InlineData(11)][InlineData(12)]
    public async Task Reports_aggregate_only_assigned_doctors(int user)
    {
        using var client = await app.Login(user);
        var result = await client.GetFromJsonAsync<ReportDto>("/api/reports/summary?from=2029-12-01T00:00:00Z&to=2030-02-01T00:00:00Z");
        Assert.Single(result!.DoctorActivity);
        Assert.Equal(4, result.DoctorActivity[0].DoctorId);
        Assert.Equal(3, result.AppointmentActivity.Sum(s => s.Count));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/reports/summary?from=2029-12-01T00:00:00Z&to=2030-02-01T00:00:00Z&doctorId=1")).StatusCode);
    }

    [Fact]
    public async Task Collections_use_payment_record_date_and_sum_recorded_amount()
    {
        using var admin = await app.Login(10);
        var from = DateTime.UtcNow.AddDays(-1).ToString("O");
        var to = DateTime.UtcNow.AddDays(1).ToString("O");
        var result = await admin.GetFromJsonAsync<ReportDto>($"/api/reports/summary?from={from}&to={to}&paymentMethod=cash&paidTo=doctor");
        Assert.Equal(25, result!.RecordedCollections.Sum(c => c.Amount));
        Assert.Equal(2, result.DoctorActivity.Count);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.GetAsync("/api/reports/summary?from=2020-01-01T00:00:00Z&to=2030-01-01T00:00:00Z")).StatusCode);
    }
}
