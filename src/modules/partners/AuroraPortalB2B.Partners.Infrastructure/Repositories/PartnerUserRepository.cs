using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Repositories;

public sealed class PartnerUserRepository(PartnersDbContext dbContext) : IPartnerUserRepository
{
    public Task<PartnerUser?> GetByIdAsync(Guid id, bool includeInactive = false, CancellationToken cancellationToken = default)
        => dbContext.PartnerUsers
            .Where(user => includeInactive || user.Status == PartnerUserStatus.Active)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<PartnerUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => dbContext.PartnerUsers
            .FirstOrDefaultAsync(user => user.Email.Value == email.Value, cancellationToken);

    public Task<PartnerUser?> GetByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
        => dbContext.PartnerUsers
            .FirstOrDefaultAsync(user => user.KeycloakUserId == keycloakUserId, cancellationToken);

    public async Task<IReadOnlyList<PartnerUser>> ListByPartnerIdAsync(Guid partnerId, int limit, int offset, bool includeInactive = false, CancellationToken cancellationToken = default)
        => await dbContext.PartnerUsers
            .Where(user => includeInactive || user.Status == PartnerUserStatus.Active)
            .Where(user => user.PartnerId == partnerId)
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Skip(Math.Max(0, offset))
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PartnerUser user, CancellationToken cancellationToken = default)
        => await dbContext.PartnerUsers.AddAsync(user, cancellationToken);
}
