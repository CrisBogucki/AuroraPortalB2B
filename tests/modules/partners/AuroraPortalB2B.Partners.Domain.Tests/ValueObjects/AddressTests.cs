using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Constructor_ShouldTrimValues()
    {
        // arrange
        var country = " PL ";
        var city = " Krakow ";
        var street = " Street 1 ";
        var postal = " 30-001 ";

        // act
        var address = new Address(country, city, street, postal);

        // assert
        address.CountryCode.Should().Be("PL");
        address.City.Should().Be("Krakow");
        address.Street.Should().Be("Street 1");
        address.PostalCode.Should().Be("30-001");
    }

    [Theory]
    [InlineData("", "City", "Street", "00-000")]
    [InlineData("PL", "", "Street", "00-000")]
    [InlineData("PL", "City", "", "00-000")]
    [InlineData("PL", "City", "Street", "")]
    public void Constructor_ShouldThrowForMissingFields(string country, string city, string street, string postal)
    {
        // arrange
        var act = () => new Address(country, city, street, postal);

        // act
        var result = act;

        // assert
        result.Should().Throw<ArgumentException>();
    }
}
