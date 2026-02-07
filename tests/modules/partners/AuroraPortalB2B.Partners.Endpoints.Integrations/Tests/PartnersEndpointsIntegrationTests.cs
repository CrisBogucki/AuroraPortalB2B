using System.Net;
using System.Linq;
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
using AuroraPortalB2B.Core.Mediator.Authorization;
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
            "kc-user-1",
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
            "kc-user-1",
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

    [Fact]
    public async Task DeactivatePartner_ShouldReturnNoContentAndSetStatus()
    {
        // arrange
        var dbName = nameof(DeactivatePartner_ShouldReturnNoContentAndSetStatus);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Delta");
        var userRequest = new CreatePartnerUserRequest(
            "kc-user-1",
            "user@acme.com",
            "Jan",
            "Nowak");
        var createUserResponse = await client.PostAsJsonAsync($"/api/v1/partners/{partnerId}/users", userRequest);
        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await createUserResponse.Content.ReadFromJsonAsync<CreatePartnerUserResponse>();
        createdUser.Should().NotBeNull();
        var userId = createdUser!.Id;

        // act
        var deactivateResponse = await client.DeleteAsync($"/api/v1/partners/{partnerId}");
        var getResponse = await client.GetAsync($"/api/v1/partners/{partnerId}?includeInactive=true");
        var listUsersResponse = await client.GetAsync($"/api/v1/partners/{partnerId}/users?includeInactive=true");

        // assert
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<PartnerDetailsDto>();
        dto.Should().NotBeNull();
        dto!.Status.Should().Be("Inactive");

        listUsersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await listUsersResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var status = doc.RootElement.GetProperty("items")
            .EnumerateArray()
            .First(item => item.GetProperty("id").GetGuid() == userId)
            .GetProperty("status")
            .GetString();
        status.Should().Be("Inactive");
    }

    [Fact]
    public async Task DeactivatePartnerUser_ShouldReturnNoContentAndSetStatus()
    {
        // arrange
        var dbName = nameof(DeactivatePartnerUser_ShouldReturnNoContentAndSetStatus);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Epsilon");

        var request = new CreatePartnerUserRequest(
            "kc-user-1",
            "user@acme.com",
            "Jan",
            "Nowak");

        var createUserResponse = await client.PostAsJsonAsync($"/api/v1/partners/{partnerId}/users", request);
        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await createUserResponse.Content.ReadFromJsonAsync<CreatePartnerUserResponse>();
        createdUser.Should().NotBeNull();
        var userId = createdUser!.Id;

        // act
        var deactivateResponse = await client.DeleteAsync($"/api/v1/partners/{partnerId}/users/{userId}");
        var listResponse = await client.GetAsync($"/api/v1/partners/{partnerId}/users?includeInactive=true");

        // assert
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await listResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var status = doc.RootElement.GetProperty("items")
            .EnumerateArray()
            .First(item => item.GetProperty("id").GetGuid() == userId)
            .GetProperty("status")
            .GetString();
        status.Should().Be("Inactive");
    }

    [Fact]
    public async Task UpdatePartner_ShouldReturnNoContentAndUpdateDetails()
    {
        // arrange
        var dbName = nameof(UpdatePartner_ShouldReturnNoContentAndUpdateDetails);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Zeta");

        var updateRequest = new UpdatePartnerRequest(
            "Zeta Updated",
            "1234563218",
            "852163975",
            new AddressDto("PL", "Warsaw", "Main 2", "00-001"));

        // act
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/partners/{partnerId}", updateRequest);
        var getResponse = await client.GetAsync($"/api/v1/partners/{partnerId}");

        // assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<PartnerDetailsDto>();
        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Zeta Updated");
        dto.Address.Should().NotBeNull();
        dto.Address!.City.Should().Be("Warsaw");
    }

    [Fact]
    public async Task UpdatePartnerUser_ShouldReturnNoContentAndUpdateDetails()
    {
        // arrange
        var dbName = nameof(UpdatePartnerUser_ShouldReturnNoContentAndUpdateDetails);
        await using var app = BuildApp(dbName);
        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        var partnerId = await CreatePartnerAsync(client, "Eta");

        var createUserRequest = new CreatePartnerUserRequest(
            "kc-user-1",
            "user@acme.com",
            "Jan",
            "Nowak");

        var createUserResponse = await client.PostAsJsonAsync($"/api/v1/partners/{partnerId}/users", createUserRequest);
        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await createUserResponse.Content.ReadFromJsonAsync<CreatePartnerUserResponse>();
        createdUser.Should().NotBeNull();
        var userId = createdUser!.Id;

        var updateUserRequest = new UpdatePartnerUserRequest(
            "new@acme.com",
            "Anna",
            "Kowalska");

        // act
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/partners/{partnerId}/users/{userId}", updateUserRequest);
        var listResponse = await client.GetAsync($"/api/v1/partners/{partnerId}/users");

        // assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var stream = await listResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var userElement = doc.RootElement.GetProperty("items")
            .EnumerateArray()
            .First(item => item.GetProperty("id").GetGuid() == userId);
        userElement.GetProperty("email").GetString().Should().Be("new@acme.com");
        userElement.GetProperty("firstName").GetString().Should().Be("Anna");
        userElement.GetProperty("lastName").GetString().Should().Be("Kowalska");
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
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
            .AddPolicy(PermissionPolicies.PartnersRead.Name, policy =>
                policy.RequireClaim(PermissionNames.ClaimType, PermissionNames.PartnersRead))
            .AddPolicy(PermissionPolicies.PartnersWrite.Name, policy =>
                policy.RequireClaim(PermissionNames.ClaimType, PermissionNames.PartnersWrite))
            .AddPolicy(PermissionPolicies.PartnerUsersRead.Name, policy =>
                policy.RequireClaim(PermissionNames.ClaimType, PermissionNames.PartnerUsersRead))
            .AddPolicy(PermissionPolicies.PartnerUsersWrite.Name, policy =>
                policy.RequireClaim(PermissionNames.ClaimType, PermissionNames.PartnerUsersWrite))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("Test")
                .RequireAuthenticatedUser()
                .Build());
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
        builder.Services.AddScoped<IValidator<UpdatePartnerRequest>, UpdatePartnerRequestValidator>();
        builder.Services.AddScoped<IValidator<UpdatePartnerUserRequest>, UpdatePartnerUserRequestValidator>();

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
        api.MapPartnerUsersEndpoints();

        return app;
    }
}
