using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.ValueObjects;

public sealed class RegonTests
{
    [Fact]
    public void Constructor_ShouldAcceptValidRegon()
    {
        // arrange
        var value = "123456785";

        // act
        var regon = new Regon(value);

        // assert
        regon.Value.Should().Be("123456785");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("abcdefghijk")]
    public void Constructor_ShouldThrowForInvalidRegon(string value)
    {
        // arrange
        var act = () => new Regon(value);

        // act
        var result = act;

        // assert
        result.Should().Throw<ArgumentException>();
    }
}
