using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Infrastructure.Persistence;

namespace AuroraPortalB2B.Partners.Infrastructure.Repositories;

public sealed class EfUnitOfWork(PartnersDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
