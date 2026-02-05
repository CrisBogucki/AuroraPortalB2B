using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Repositories;

public sealed class PartnerRepository(PartnersDbContext dbContext) : IPartnerRepository
{
    public Task<Partner?> GetByIdAsync(Guid id, bool includeInactive = false, CancellationToken cancellationToken = default)
        => dbContext.Partners
            .Where(partner => includeInactive || partner.Status == PartnerStatus.Active)
            .FirstOrDefaultAsync(partner => partner.Id == id, cancellationToken);

    public Task<Partner?> GetByNipAsync(Nip nip, CancellationToken cancellationToken = default)
        => dbContext.Partners
            .FirstOrDefaultAsync(partner => partner.Nip.Value == nip.Value, cancellationToken);

    public Task<bool> ExistsByNipAsync(Nip nip, CancellationToken cancellationToken = default)
        => dbContext.Partners
            .AnyAsync(partner => partner.Nip.Value == nip.Value, cancellationToken);

    public async Task<IReadOnlyList<Partner>> ListAsync(int limit, int offset, bool includeInactive = false, CancellationToken cancellationToken = default)
        => await dbContext.Partners
            .Where(partner => includeInactive || partner.Status == PartnerStatus.Active)
            .OrderBy(partner => partner.Name)
            .Skip(Math.Max(0, offset))
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Partner partner, CancellationToken cancellationToken = default)
        => await dbContext.Partners.AddAsync(partner, cancellationToken);
}
