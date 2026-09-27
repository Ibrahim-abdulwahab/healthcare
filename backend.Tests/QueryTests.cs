using System.Net.Http.Json;
using Healthcare.Api.DTOs.Consultations;
using Xunit;

namespace Healthcare.Api.Tests;

public class QueryTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Clinical_search_is_case_insensitive_scoped_and_paginated_in_SQL()
    {
        using var doctor = await app.Login(4);
        var own = await doctor.GetFromJsonAsync<ConsultationDto[]>("/api/consultations?search=FOUR&pageSize=1");
        Assert.Single(own!); Assert.Equal(101, own![0].Id);
        Assert.Empty((await doctor.GetFromJsonAsync<ConsultationDto[]>("/api/consultations?search=ONE"))!);
        Assert.Empty((await doctor.GetFromJsonAsync<ConsultationDto[]>("/api/consultations?search=FOUR&pageSize=1&page=2"))!);
    }
}
