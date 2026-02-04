using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.ValueObjects;

public sealed class NipTests
{
    [Fact]
    public void Constructor_ShouldAcceptValidNip()
    {
        // arrange
        var value = "1234563218";

        // act
        var nip = new Nip(value);

        // assert
        nip.Value.Should().Be("1234563218");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890")]
    [InlineData("abcdefghij")]
    public void Constructor_ShouldThrowForInvalidNip(string value)
    {
        // arrange
        var act = () => new Nip(value);

        // act
        var result = act;

        // assert
        result.Should().Throw<ArgumentException>();
    }
}
