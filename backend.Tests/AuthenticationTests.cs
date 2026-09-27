using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Healthcare.Api.Tests;

public class AuthenticationTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture app;
    public AuthenticationTests(ApiFixture app) => this.app = app;

    [Fact]
    public async Task Login_and_protected_identity_use_real_JWT()
    {
        using var doctor = await app.Login(4);
        Assert.Equal(HttpStatusCode.OK, (await doctor.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await doctor.GetAsync("/api/users")).StatusCode);
        using var anonymous = app.CreateClient(new() { BaseAddress = new("https://localhost") });
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/auth/login", new { email = "user4@test.local", password = "wrong" })).StatusCode);
    }
}
