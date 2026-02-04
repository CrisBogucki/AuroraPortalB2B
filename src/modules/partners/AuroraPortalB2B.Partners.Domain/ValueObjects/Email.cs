namespace AuroraPortalB2B.Partners.Domain.ValueObjects;

public sealed record Email
{
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (!IsValidFormat(trimmed))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }

        Value = trimmed;
    }

    public string Value { get; }

    public override string ToString() => Value;

    private static bool IsValidFormat(string value)
    {
        var atIndex = value.IndexOf('@');
        if (atIndex <= 0 || atIndex != value.LastIndexOf('@'))
        {
            return false;
        }

        var dotIndex = value.LastIndexOf('.');
        return dotIndex > atIndex + 1 && dotIndex < value.Length - 1;
    }
}
