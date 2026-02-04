using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Host.Configuration;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Host.Tests.Unit;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHostServices_ShouldRegisterPartnersModule()
    {
        // arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:partners"] = "Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres"
            })
            .Build();

        // act
        services.AddHostServices(configuration);

        // assert
        using var provider = services.BuildServiceProvider();
        provider.GetService<ISender>().Should().NotBeNull();
        provider.GetService<IValidator<CreatePartnerRequest>>().Should().BeOfType<CreatePartnerRequestValidator>();
    }
}
