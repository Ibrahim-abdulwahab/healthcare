using System.Net;
using System.Net.Http.Json;
using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.DTOs.Availability;
using Healthcare.Api.Services.Availability;
using Healthcare.Api.Services.Appointments;
using Healthcare.Api.Infrastructure;
using Xunit;

namespace Healthcare.Api.Tests;

public class SchedulingTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Theory]
    [InlineData("doctors", 4, 10)][InlineData("locations", 2, 11)][InlineData("doctor-locations", 2, 12)]
    [InlineData("visit-types", 1, 13)][InlineData("schedules", 2, 14)]
    public async Task Inactive_resources_block_new_bookings_but_never_cancellation(string resource, int id, int week)
    {
        using var admin = await app.Login(10);
        using var staff = await app.Login(11);
        var current = (await admin.GetFromJsonAsync<System.Text.Json.Nodes.JsonObject>($"/api/{resource}/{id}"))!;
        var start = app.Start.AddDays(week * 7);
        var create = await staff.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = start });
        create.EnsureSuccessStatusCode(); var appointment = (await create.Content.ReadFromJsonAsync<AppointmentDto>())!;
        try
        {
            current["isActive"] = false;
            (await admin.PutAsJsonAsync($"/api/{resource}/{id}", current)).EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Conflict, (await staff.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = start.AddHours(1) })).StatusCode);
            (await staff.PatchAsJsonAsync($"/api/appointments/{appointment.Id}/status", new { status = "cancelled" })).EnsureSuccessStatusCode();
        }
        finally { current["isActive"] = true; (await admin.PutAsJsonAsync($"/api/{resource}/{id}", current)).EnsureSuccessStatusCode(); }
    }

    [Fact]
    public void Different_duration_and_step_deduplicate_and_respect_touching_boundaries()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
        var window = new SlotCalculator.Window(new(9, 0), new(13, 0), 20);
        var slots = SlotCalculator.Calculate(new(2030, 1, 7), 30, zone, [window, window], [], [new(app.Start.AddMinutes(30), app.Start.AddHours(1))]);
        Assert.Contains(slots, s => s.StartDatetime == app.Start);
        Assert.DoesNotContain(slots, s => s.StartDatetime == app.Start.AddMinutes(20));
        Assert.Contains(slots, s => s.StartDatetime == app.Start.AddHours(1));
        Assert.Equal(slots.Count, slots.Select(s => s.StartDatetime).Distinct().Count());
        Assert.All(slots, s => Assert.Equal(TimeSpan.FromMinutes(30), s.EndDatetime - s.StartDatetime));
    }

    [Theory]
    [InlineData(0)][InlineData(-1)][InlineData(1440)]
    public void Invalid_intervals_never_loop(int step)
    {
        Assert.Empty(SlotCalculator.Calculate(new(2030, 1, 7), 30, TimeZoneInfo.Utc, [new(new(9, 0), new(23, 59), step)], [], []));
        Assert.Throws<ArgumentException>(() => SlotCalculator.Calculate(new(2030, 1, 7), step, TimeZoneInfo.Utc, [], [], []));
    }

    [Theory]
    [InlineData("booked", "completed")][InlineData("completed", "arrived")][InlineData("cancelled", "booked")]
    public void Lifecycle_rejects_jumps_and_terminal_exits(string before, string after) => Assert.Throws<ApiException>(() => AppointmentLifecycle.Validate(before, after));

    [Fact]
    public async Task Availability_booking_reschedule_cancellation_and_attribution_agree()
    {
        using var staff = await app.Login(11);
        var date = "2030-01-14";
        var available = await staff.GetFromJsonAsync<AvailableSlotDto[]>($"/api/availability?doctorId=4&locationId=2&visitTypeId=1&date={date}");
        Assert.NotEmpty(available!);
        var start = available![0].StartDatetime;
        var response = await staff.PostAsJsonAsync("/api/appointments", new { patientId = 5, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = start, endDatetime = start.AddHours(7), createdBy = 10 });
        response.EnsureSuccessStatusCode();
        var appointment = (await response.Content.ReadFromJsonAsync<AppointmentDto>())!;
        Assert.Equal(11, appointment.CreatedBy);
        Assert.Equal(start.AddMinutes(30), appointment.EndDatetime);
        var otherLocation = await staff.GetFromJsonAsync<AvailableSlotDto[]>($"/api/availability?doctorId=4&locationId=3&visitTypeId=1&date={date}");
        Assert.DoesNotContain(otherLocation!, s => s.StartDatetime == start);
        var offGrid = await staff.PutAsJsonAsync($"/api/appointments/{appointment.Id}/reschedule", new { doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = start.AddMinutes(7) });
        Assert.Equal(HttpStatusCode.Conflict, offGrid.StatusCode);
        var move = await staff.PutAsJsonAsync($"/api/appointments/{appointment.Id}/reschedule", new { doctorId = 4, locationId = 3, visitTypeId = 1, startDatetime = start.AddHours(1) });
        move.EnsureSuccessStatusCode();
        using var doctor = await app.Login(4);
        (await doctor.PostAsJsonAsync("/api/schedule-exceptions", new { doctorLocationId = 3, date, startTime = "09:00", endTime = "17:00", isAvailable = false })).EnsureSuccessStatusCode();
        (await staff.PatchAsJsonAsync($"/api/appointments/{appointment.Id}/status", new { status = "cancelled" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PatchAsJsonAsync($"/api/appointments/{appointment.Id}/status", new { status = "arrived" })).StatusCode);
        var freed = await staff.GetFromJsonAsync<AvailableSlotDto[]>($"/api/availability?doctorId=4&locationId=2&visitTypeId=1&date={date}");
        Assert.Contains(freed!, s => s.StartDatetime == start.AddHours(1));
    }

    [Fact]
    public async Task Available_exception_adds_hours_without_a_weekday_schedule()
    {
        using var doctor = await app.Login(4);
        using var staff = await app.Login(12);
        (await doctor.PostAsJsonAsync("/api/schedule-exceptions", new { doctorLocationId = 2, date = "2030-01-08", startTime = "09:00", endTime = "11:00", isAvailable = true })).EnsureSuccessStatusCode();
        var slots = await staff.GetFromJsonAsync<AvailableSlotDto[]>("/api/availability?doctorId=4&locationId=2&visitTypeId=1&date=2030-01-08");
        Assert.NotEmpty(slots!);
        Assert.Equal(TimeSpan.FromMinutes(20), slots![1].StartDatetime - slots[0].StartDatetime);
        (await staff.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = slots[0].StartDatetime })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Concurrent_cross_location_bookings_allow_exactly_one_winner()
    {
        using var first = await app.Login(11);
        using var second = await app.Login(12);
        var start = app.Start.AddDays(21);
        var results = await Task.WhenAll(
            first.PostAsJsonAsync("/api/appointments", new { patientId = 1, doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = start }),
            second.PostAsJsonAsync("/api/appointments", new { patientId = 4, doctorId = 4, locationId = 3, visitTypeId = 1, startDatetime = start }));
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Visit_locks_rescheduling_and_doctor_cannot_move_any_appointment()
    {
        using var staff = await app.Login(11);
        using var doctor = await app.Login(4);
        var payload = new { doctorId = 4, locationId = 2, visitTypeId = 1, startDatetime = app.Start };
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PutAsJsonAsync("/api/appointments/101/reschedule", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.PutAsJsonAsync("/api/appointments/101/reschedule", payload)).StatusCode);
    }
}
