using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Abstractions.Repositories;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Partner?> GetByNipAsync(Nip nip, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNipAsync(Nip nip, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Partner>> ListAsync(int limit, int offset, CancellationToken cancellationToken = default);
    Task AddAsync(Partner partner, CancellationToken cancellationToken = default);
}
