namespace AuroraPortalB2B.Partners.Domain.ValueObjects;

public sealed record Regon
{
    public Regon(string value)
    {
        var digits = Normalize(value, "REGON");
        if (digits.Length is not (9 or 14) || !IsValid(digits))
        {
            throw new ArgumentException("REGON is invalid.", nameof(value));
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
        if (digits.Length == 9)
        {
            int[] weights = [8, 9, 2, 3, 4, 5, 6, 7];
            return Check(digits, weights, 8);
        }

        int[] weights14 = [2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8];
        return Check(digits, weights14, 13);
    }

    private static bool Check(string digits, int[] weights, int checkIndex)
    {
        var sum = weights.Select((t, i) => t * (digits[i] - '0')).Sum();

        var checksum = sum % 11;
        if (checksum == 10)
        {
            checksum = 0;
        }

        return checksum == digits[checkIndex] - '0';
    }
}
