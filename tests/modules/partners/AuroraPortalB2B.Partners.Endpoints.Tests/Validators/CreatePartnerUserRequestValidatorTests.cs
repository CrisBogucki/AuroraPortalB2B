using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Endpoints.Tests.Validators;

public sealed class CreatePartnerUserRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldFailForInvalidEmail()
    {
        // arrange
        var validator = new CreatePartnerUserRequestValidator();
        var request = new CreatePartnerUserRequest("bad", "Jan", "Kowalski");

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldPassForValidRequest()
    {
        // arrange
        var validator = new CreatePartnerUserRequestValidator();
        var request = new CreatePartnerUserRequest("user@acme.com", "Jan", "Kowalski");

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeTrue();
    }
}
