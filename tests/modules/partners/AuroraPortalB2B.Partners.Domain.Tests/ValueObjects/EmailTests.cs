using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Constructor_ShouldTrimAndStoreValue()
    {
        // arrange
        var value = "  test@example.com ";

        // act
        var email = new Email(value);

        // assert
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("no-at-sign.com")]
    [InlineData("a@b")]
    public void Constructor_ShouldThrowForInvalidFormat(string value)
    {
        // arrange
        var act = () => new Email(value);

        // act
        var result = act;

        // assert
        result.Should().Throw<ArgumentException>();
    }
}
