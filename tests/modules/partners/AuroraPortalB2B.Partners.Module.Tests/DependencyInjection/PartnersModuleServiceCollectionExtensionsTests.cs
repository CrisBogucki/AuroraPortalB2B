using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using AuroraPortalB2B.Partners.Module.DependencyInjection;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Partners.Module.Tests.DependencyInjection;

public sealed class PartnersModuleServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPartnersModule_ShouldRegisterMediatorAndValidators()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddPartnersModule("Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres");

        // assert
        using var provider = services.BuildServiceProvider();

        provider.GetService<ISender>().Should().NotBeNull();
        provider.GetService<IValidator<CreatePartnerRequest>>().Should().BeOfType<CreatePartnerRequestValidator>();
        provider.GetService<IValidator<CreatePartnerUserRequest>>().Should().BeOfType<CreatePartnerUserRequestValidator>();
    }
}
