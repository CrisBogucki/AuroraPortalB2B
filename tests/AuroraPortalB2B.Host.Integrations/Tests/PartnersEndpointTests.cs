using System.Net;
using System.Text.Json;
using AuroraPortalB2B.Host.Integrations.Helper;
using FluentAssertions;

namespace AuroraPortalB2B.Host.Integrations.Tests;

public sealed class PartnersEndpointTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPartners_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }
}
