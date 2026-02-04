namespace AuroraPortalB2B.Partners.Domain.ValueObjects;

public sealed record Nip
{
    public Nip(string value)
    {
        var digits = Normalize(value, "NIP");
        if (digits.Length != 10 || !IsValid(digits))
        {
            throw new ArgumentException("NIP is invalid.", nameof(value));
        }

        Value = digits;
    }

    public string Value { get; }

    public override string ToString() => Value;

    private static string Normalize(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} is required.", nameof(value));
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            throw new ArgumentException($"{field} must contain digits.", nameof(value));
        }

        return digits;
    }

    private static bool IsValid(string digits)
    {
        int[] weights = [6, 5, 7, 2, 3, 4, 5, 6, 7];
        var sum = weights.Select((t, i) => t * (digits[i] - '0')).Sum();

        var checksum = sum % 11;
        if (checksum == 10)
        {
            return false;
        }

        return checksum == digits[9] - '0';
    }
}
