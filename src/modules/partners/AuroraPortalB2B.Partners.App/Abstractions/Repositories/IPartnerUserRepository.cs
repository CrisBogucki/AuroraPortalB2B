using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Abstractions.Repositories;

public interface IPartnerUserRepository
{
    Task<PartnerUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartnerUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartnerUser>> ListByPartnerIdAsync(Guid partnerId, int limit, int offset, CancellationToken cancellationToken = default);
    Task AddAsync(PartnerUser user, CancellationToken cancellationToken = default);
}
