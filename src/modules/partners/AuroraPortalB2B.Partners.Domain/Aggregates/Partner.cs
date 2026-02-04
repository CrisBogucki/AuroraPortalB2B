using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.Domain.Aggregates;

public sealed class Partner
{
    private readonly List<PartnerUser> _users = [];

    private Partner()
    {
        Name = string.Empty;
        Nip = null!;
    }

    public Partner(Guid id, string name, Nip nip, Regon? regon = null, Address? address = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Partner name is required.", nameof(name))
            : name.Trim();
        Nip = nip ?? throw new ArgumentNullException(nameof(nip));
        Regon = regon;
        Address = address;
        Status = PartnerStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Nip Nip { get; private set; }
    public Regon? Regon { get; private set; }
    public Address? Address { get; private set; }
    public PartnerStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<PartnerUser> Users => _users.AsReadOnly();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Partner name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    public void ChangeNip(Nip nip) => Nip = nip ?? throw new ArgumentNullException(nameof(nip));

    public void ChangeRegon(Regon? regon) => Regon = regon;

    public void ChangeAddress(Address? address) => Address = address;

    public void Deactivate() => Status = PartnerStatus.Inactive;

    public PartnerUser AddUser(Guid id, Email email, string firstName, string lastName)
    {
        var user = new PartnerUser(id, Id, email, firstName, lastName);
        _users.Add(user);
        return user;
    }
}
