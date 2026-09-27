using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Healthcare.Api.Tests;
using Healthcare.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Local verification only. A fresh random database is seeded with test accounts, then removed.
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
await using var fixture = new ApiFixture();
var children = new List<Process>();
try
{
    await fixture.InitializeAsync();
    await using (var db = new HealthcareDbContext(new DbContextOptionsBuilder<HealthcareDbContext>().UseNpgsql(fixture.ConnectionString).Options))
    {
        if (db.Database.HasPendingModelChanges()) throw new InvalidOperationException("Unexpected EF model changes detected.");
        Console.WriteLine("EF snapshot check: no pending model changes.");
    }
    var apiPort = Port(); var uiPort = Port();
    var apiUrl = $"http://127.0.0.1:{apiPort}"; var uiUrl = $"http://127.0.0.1:{uiPort}";
    var apiEnvironment = new Dictionary<string, string> { ["ASPNETCORE_ENVIRONMENT"] = "Testing", ["ASPNETCORE_URLS"] = apiUrl,
        ["ConnectionStrings__DefaultConnection"] = fixture.ConnectionString, ["Jwt__Key"] = "test-only-" + new string('x', 64),
        ["Jwt__Issuer"] = "Healthcare.Tests", ["Jwt__Audience"] = "Healthcare.Tests", ["Jwt__ExpirationMinutes"] = "60",
        ["Healthcare__TimeZone"] = "Asia/Beirut", ["Logging__LogLevel__Default"] = "Warning" };
    Start("dotnet", [Path.Combine(root, "backend/bin/Debug/net10.0/Healthcare.Api.dll")], root, apiEnvironment);
    Start("node", [Path.Combine(root, "frontend/node_modules/vite/bin/vite.js"), "--host", "127.0.0.1", "--port", uiPort.ToString(), "--strictPort"], Path.Combine(root, "frontend"), new() { ["API_PROXY_TARGET"] = apiUrl });
    using var http = new HttpClient();
    await Ready(apiUrl + "/health"); await Ready(uiUrl);
    Console.WriteLine("Real API and React servers started against the isolated test database.");
    var tests = Start("node", [Path.Combine(root, "frontend/node_modules/@playwright/test/cli.js"), "test"], Path.Combine(root, "frontend"), new() { ["UI_BASE_URL"] = uiUrl });
    await tests.WaitForExitAsync();
    Environment.ExitCode = tests.ExitCode;

    async Task Ready(string url)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            try { if ((await http.GetAsync(url)).IsSuccessStatusCode) return; } catch (HttpRequestException) { }
            await Task.Delay(200);
        }
        throw new InvalidOperationException("Verification server did not become ready: " + url);
    }
}
finally
{
    foreach (var process in children.AsEnumerable().Reverse()) { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } process.Dispose(); }
    await ((IAsyncLifetime)fixture).DisposeAsync();
}

Process Start(string executable, string[] args, string cwd, Dictionary<string, string> environment)
{
    var start = new ProcessStartInfo(executable) { WorkingDirectory = cwd, UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var arg in args) start.ArgumentList.Add(arg);
    foreach (var item in environment) start.Environment[item.Key] = item.Value;
    var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start verification process.");
    process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
    process.BeginOutputReadLine(); process.BeginErrorReadLine();
    children.Add(process); return process;
}
static int Port() { using var socket = new TcpListener(IPAddress.Loopback, 0); socket.Start(); return ((IPEndPoint)socket.LocalEndpoint).Port; }
