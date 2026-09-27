using System.Net.Http.Headers;
using System.Net.Http.Json;
using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Healthcare.Api.Tests;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string database = "healthcarev2_test_" + Guid.NewGuid().ToString("N");
    private string connection = "";
    public string ConnectionString => connection;
    private readonly Dictionary<int, string> tokens = [];
    public const string Password = "Local-test-password!42";
    public DateTime Start = new(2030, 1, 7, 7, 0, 0, DateTimeKind.Utc); // Monday, 09:00 Beirut

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.GetFullPath("../../../../backend", AppContext.BaseDirectory));
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connection,
            ["Jwt:Key"] = "test-only-" + new string('x', 64),
            ["Jwt:Issuer"] = "Healthcare.Tests", ["Jwt:Audience"] = "Healthcare.Tests",
            ["Jwt:ExpirationMinutes"] = "60", ["Healthcare:TimeZone"] = "Asia/Beirut", ["Logging:LogLevel:Default"] = "Warning"
        }));
        builder.ConfigureServices(services => services.AddDbContext<HealthcareDbContext>(o => o.UseNpgsql(connection)));
    }

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("../../../../backend/appsettings.Development.json", AppContext.BaseDirectory), optional: true)
            .AddEnvironmentVariables().Build();
        var source = Environment.GetEnvironmentVariable("HEALTHCARE_TEST_CONNECTION") ?? config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Set HEALTHCARE_TEST_CONNECTION to a local PostgreSQL connection with CREATE DATABASE permission.");
        var cs = new NpgsqlConnectionStringBuilder(source) { Database = database };
        connection = cs.ConnectionString;
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthcareDbContext>();
        await db.Database.EnsureCreatedAsync();
        db.Organizations.AddRange(new Organization { Id = 1, Name = "Clinic One", Type = "clinic" }, new Organization { Id = 2, Name = "Clinic Two", Type = "hospital" });
        var hasher = new PasswordHasher<User>();
        foreach (var (id, role) in new[] { (1, "doctor"), (4, "doctor"), (10, "admin"), (11, "staff"), (12, "staff"), (13, "staff") })
        {
            var user = new User { Id = id, Name = $"Test {id}", Email = $"user{id}@test.local", Role = role, PasswordHash = "" };
            user.PasswordHash = hasher.HashPassword(user, Password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync();
        db.Doctors.AddRange(new Doctor { Id = 1, OrganizationId = 1, FirstName = "One", LastName = "Doctor" }, new Doctor { Id = 4, OrganizationId = 2, FirstName = "Four", LastName = "Doctor" });
        db.Locations.AddRange(new Location { Id = 1, OrganizationId = 1, Name = "Location One" }, new Location { Id = 2, OrganizationId = 2, Name = "Location Two" }, new Location { Id = 3, OrganizationId = 2, Name = "Location Three" });
        await db.SaveChangesAsync();
        (await db.Users.FindAsync(11))!.StaffDoctorId = 4;
        (await db.Users.FindAsync(12))!.StaffOrganizationId = 2;
        (await db.Users.FindAsync(13))!.StaffDoctorId = 1;
        db.DoctorLocations.AddRange(new DoctorLocation { Id = 1, DoctorId = 1, LocationId = 1 }, new DoctorLocation { Id = 2, DoctorId = 4, LocationId = 2 }, new DoctorLocation { Id = 3, DoctorId = 4, LocationId = 3 });
        db.Patients.AddRange(Enumerable.Range(1, 5).Select(id => new Patient { Id = id, Mrn = $"TEST-{id}", FirstName = $"Patient {id}", LastName = "Test" }));
        db.VisitTypes.Add(new VisitType { Id = 1, Name = "Consultation", DefaultDuration = 30 });
        await db.SaveChangesAsync();
        db.Schedules.AddRange(Enumerable.Range(1, 3).Select(id => new Schedule { Id = id, DoctorLocationId = id, DayOfWeek = 1, StartTime = new(9, 0), EndTime = new(17, 0), SlotDuration = 20 }));
        await db.SaveChangesAsync();
        db.ItemCategories.Add(new ItemCategory { Id = 1, Name = "Medication" });
        db.Items.Add(new Item { Id = 1, CategoryId = 1, Code = "TEST", Name = "Test item", Type = "medication", Unit = "tablet" });
        db.Appointments.AddRange(
            new Appointment { Id = 100, DoctorId = 1, LocationId = 1, PatientId = 1, VisitTypeId = 1, StartDatetime = Start.AddDays(-7), EndDatetime = Start.AddDays(-7).AddMinutes(30), CreatedBy = 10 },
            new Appointment { Id = 101, DoctorId = 4, LocationId = 2, PatientId = 1, VisitTypeId = 1, StartDatetime = Start.AddDays(-7), EndDatetime = Start.AddDays(-7).AddMinutes(30), CreatedBy = 10 },
            new Appointment { Id = 102, DoctorId = 1, LocationId = 1, PatientId = 2, VisitTypeId = 1, StartDatetime = Start.AddDays(-7).AddHours(1), EndDatetime = Start.AddDays(-7).AddHours(1.5), CreatedBy = 10 },
            new Appointment { Id = 103, DoctorId = 4, LocationId = 2, PatientId = 3, VisitTypeId = 1, Status = "cancelled", StartDatetime = Start.AddDays(-7).AddHours(2), EndDatetime = Start.AddDays(-7).AddHours(2.5), CreatedBy = 10 },
            new Appointment { Id = 104, DoctorId = 4, LocationId = 2, PatientId = 4, VisitTypeId = 1, Status = "cancelled", StartDatetime = Start.AddDays(-7).AddHours(3), EndDatetime = Start.AddDays(-7).AddHours(3.5), CreatedBy = 10 });
        await db.SaveChangesAsync();
        db.Visits.AddRange(new Visit { Id = 100, AppointmentId = 100, DoctorId = 1, LocationId = 1, PatientId = 1 }, new Visit { Id = 101, AppointmentId = 101, DoctorId = 4, LocationId = 2, PatientId = 1 }, new Visit { Id = 104, AppointmentId = 104, DoctorId = 4, LocationId = 2, PatientId = 4 });
        await db.SaveChangesAsync();
        db.Consultations.AddRange(new Consultation { Id = 100, VisitId = 100, CreatedBy = 1, Diagnosis = "Doctor one only" }, new Consultation { Id = 101, VisitId = 101, CreatedBy = 4, Diagnosis = "Doctor four only" });
        db.Notifications.Add(new Notification { Id = 1, UserId = 4, Type = "in-app", Message = "Private notice" });
        db.PaymentRecords.Add(new PaymentRecord { Id = 1, VisitId = 101, CollectedBy = 11, Amount = 25, PaymentMethod = "cash", PaidTo = "doctor", PaymentStatus = "paid" });
        await db.SaveChangesAsync();
        db.Prescriptions.AddRange(new Prescription { Id = 100, ConsultationId = 100, DoctorId = 1, PatientId = 1, PrescriptionDate = new(2030, 1, 1) }, new Prescription { Id = 101, ConsultationId = 101, DoctorId = 4, PatientId = 1, PrescriptionDate = new(2030, 1, 1) });
        db.Files.Add(new HealthcareFile { Id = 1, TableName = "consultations", RecordId = 101, FileName = "Reference", FileUrl = "https://example.invalid/reference.pdf", UploadedBy = 4 });
        await db.SaveChangesAsync();
        // Explicit fixture IDs must not collide with API-generated identities.
        // Table names are a fixed test-only whitelist, never request input.
#pragma warning disable EF1002
        foreach (var table in new[] { "organizations", "users", "locations", "doctor_locations", "patients", "visit_types", "schedules", "appointments", "visits", "consultations", "prescriptions", "files", "notifications", "payment_records", "items", "item_categories" })
            await db.Database.ExecuteSqlRawAsync($"SELECT setval(pg_get_serial_sequence('{table}', 'id'), (SELECT MAX(id) FROM {table}))");
#pragma warning restore EF1002
    }

    public async Task<HttpClient> Login(int id)
    {
        var client = CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        if (tokens.TryGetValue(id, out var token)) { client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); return client; }
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = $"user{id}@test.local", password = Password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Healthcare.Api.DTOs.Auth.LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
        tokens[id] = result.Token;
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await using var db = new HealthcareDbContext(new DbContextOptionsBuilder<HealthcareDbContext>().UseNpgsql(connection).Options);
        if (!database.StartsWith("healthcarev2_test_") || database.Length != 50) throw new InvalidOperationException("Unsafe test database name.");
        await db.Database.EnsureDeletedAsync();
        await DisposeAsync();
    }
}
