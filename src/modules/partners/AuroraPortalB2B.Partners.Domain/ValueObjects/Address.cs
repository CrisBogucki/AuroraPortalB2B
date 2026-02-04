namespace AuroraPortalB2B.Partners.Domain.ValueObjects;

public sealed record Address
{
    public Address(string countryCode, string city, string street, string postalCode)
    {
        CountryCode = Normalize(countryCode, "Country code");
        City = Normalize(city, "City");
        Street = Normalize(street, "Street");
        PostalCode = Normalize(postalCode, "Postal code");
    }

    public string CountryCode { get; }
    public string City { get; }
    public string Street { get; }
    public string PostalCode { get; }

    private static string Normalize(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} is required.", nameof(value));
        }

        return value.Trim();
    }
}
