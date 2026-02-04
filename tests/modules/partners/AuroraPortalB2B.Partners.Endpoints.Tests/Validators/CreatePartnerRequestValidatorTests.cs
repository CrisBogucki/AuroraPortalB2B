using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Endpoints.Validators;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Endpoints.Tests.Validators;

public sealed class CreatePartnerRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldFailForInvalidData()
    {
        // arrange
        var validator = new CreatePartnerRequestValidator();
        var request = new CreatePartnerRequest("", "123", "123", null);

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldPassForValidData()
    {
        // arrange
        var validator = new CreatePartnerRequestValidator();
        var request = new CreatePartnerRequest("Acme", "1234563218", null, null);

        // act
        var result = validator.Validate(request);

        // assert
        result.IsValid.Should().BeTrue();
    }
}
