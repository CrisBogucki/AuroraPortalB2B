using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Asp.Versioning;
using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Extensions;
using AuroraPortalB2B.Partners.Endpoints.Integrations.Helper;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Endpoints;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuroraPortalB2B.Partners.Endpoints.Integrations.Tests;

public sealed class PartnersEndpointsIntegrationTests
{
    [Fact]
    public async Task PostAndGet_ShouldReturnCreatedAndListed()
    {
        // arrange
        var dbName = nameof(PostAndGet_ShouldReturnCreatedAndListed);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var request = new CreatePartnerRequest(
            "Acme",
            "1234563218",
            "852163975",
            new AddressDto("PL", "Krakow", "Main", "30-001"));

        // act
        var createResponse = await client.PostAsJsonAsync("/api/v1/partners", request);
        var listResponse = await client.GetAsync("/api/v1/partners");

        // assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatePartnerResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await listResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        doc.RootElement.GetProperty("total").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetPartner_ShouldReturnPartnerDetails()
    {
        // arrange
        var dbName = nameof(GetPartner_ShouldReturnPartnerDetails);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Acme");

        // act
        var response = await client.GetAsync($"/api/v1/partners/{partnerId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<PartnerDetailsDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(partnerId);
        dto.Name.Should().Be("Acme");
    }

    [Fact]
    public async Task CreatePartnerUser_ShouldReturnCreated()
    {
        // arrange
        var dbName = nameof(CreatePartnerUser_ShouldReturnCreated);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Beta");

        var request = new CreatePartnerUserRequest(
            "user@acme.com",
            "Jan",
            "Nowak");

        // act
        var response = await client.PostAsJsonAsync($"/api/v1/partners/{partnerId}/users", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreatePartnerUserResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ListPartnerUsers_ShouldReturnUsers()
    {
        // arrange
        var dbName = nameof(ListPartnerUsers_ShouldReturnUsers);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Gamma");

        var request = new CreatePartnerUserRequest(
            "user@acme.com",
            "Jan",
            "Nowak");

        var createUserResponse = await client.PostAsJsonAsync($"/api/v1/partners/{partnerId}/users", request);
        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // act
        var listResponse = await client.GetAsync($"/api/v1/partners/{partnerId}/users");

        // assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await listResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        doc.RootElement.GetProperty("total").GetInt32().Should().Be(1);
    }

    private static async Task<Guid> CreatePartnerAsync(HttpClient client, string name)
    {
        var request = new CreatePartnerRequest(
            name,
            "1234563218",
            "852163975",
            new AddressDto("PL", "Krakow", "Main", "30-001"));

        var response = await client.PostAsJsonAsync("/api/v1/partners", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<CreatePartnerResponse>();
        created.Should().NotBeNull();
        return created!.Id;
    }

    private static WebApplication BuildApp(string databaseName)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("Test")
                .RequireAuthenticatedUser()
                .Build();
        });
        builder.Services.AddLogging();
        builder.Services.AddProblemDetails();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new QueryStringApiVersionReader("api-version"));
        });

        var inMemoryProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        builder.Services.AddDbContext<PartnersDbContext>(options =>
            options.UseInMemoryDatabase(databaseName)
                .UseInternalServiceProvider(inMemoryProvider));

        builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
        builder.Services.AddScoped<IPartnerUserRepository, PartnerUserRepository>();
        builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        builder.Services.AddMediator(options =>
        {
            options.AddAssemblies(typeof(CreatePartnerCommand).Assembly);
            options.AddPipelineBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        builder.Services.AddScoped<IValidator<CreatePartnerRequest>, CreatePartnerRequestValidator>();
        builder.Services.AddScoped<IValidator<CreatePartnerUserRequest>, CreatePartnerUserRequestValidator>();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();

        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        api.MapPartnersEndpoints();

        return app;
    }
}
