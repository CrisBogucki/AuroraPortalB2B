using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.App.Abstractions.Tenancy;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using AuroraPortalB2B.Partners.Module.DependencyInjection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Partners.Module.Integrations.Tests;

public sealed class PartnersModuleIntegrationTests
{
    [Fact]
    public async Task Send_ShouldPersistAndReadPartnerUsingModuleRegistrations()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ITenantContext>(_ => new TestTenantContext());
        services.AddPartnersModule("Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres");

        var dbName = nameof(Send_ShouldPersistAndReadPartnerUsingModuleRegistrations);

        var npgsqlDescriptors = services
            .Where(d => d.ImplementationType?.Namespace?.StartsWith("Npgsql.EntityFrameworkCore.PostgreSQL") == true
                || d.ServiceType?.Namespace?.StartsWith("Npgsql.EntityFrameworkCore.PostgreSQL") == true)
            .ToList();

        foreach (var descriptor in npgsqlDescriptors)
        {
            services.Remove(descriptor);
        }

        var dbContextOptionsDescriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<PartnersDbContext>))
            .ToList();

        foreach (var descriptor in dbContextOptionsDescriptors)
        {
            services.Remove(descriptor);
        }

        var inMemoryProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        services.AddDbContext<PartnersDbContext>(options =>
            options.UseInMemoryDatabase(dbName)
                .UseInternalServiceProvider(inMemoryProvider));

        using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // act
        var createResult = await sender.Send(new CreatePartnerCommand(
            "Acme",
            "1234563218",
            null,
            null,
            null,
            null,
            null));

        var listResult = await sender.Send(new ListPartnersQuery(10, 0));

        // assert
        createResult.IsSuccess.Should().BeTrue();
        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().ContainSingle();
        listResult.Value![0].Id.Should().Be(createResult.Value);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public string TenantId { get; } = "tenant-1";
    }
}
