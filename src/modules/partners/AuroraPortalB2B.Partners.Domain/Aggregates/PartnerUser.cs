using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.Domain.Aggregates;

public sealed class PartnerUser
{
    private PartnerUser()
    {
        Email = null!;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public PartnerUser(Guid id, Guid partnerId, Email email, string firstName, string lastName)
    {
        if (partnerId == Guid.Empty)
        {
            throw new ArgumentException("Partner id is required.", nameof(partnerId));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        PartnerId = partnerId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FirstName = string.IsNullOrWhiteSpace(firstName)
            ? throw new ArgumentException("First name is required.", nameof(firstName))
            : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName)
            ? throw new ArgumentException("Last name is required.", nameof(lastName))
            : lastName.Trim();
        Status = PartnerUserStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid PartnerId { get; private set; }
    public Email Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PartnerUserStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void ChangeEmail(Email email) => Email = email ?? throw new ArgumentNullException(nameof(email));

    public void Rename(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void Deactivate() => Status = PartnerUserStatus.Inactive;
}
