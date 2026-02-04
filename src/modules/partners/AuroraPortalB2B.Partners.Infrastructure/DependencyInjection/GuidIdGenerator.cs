using AuroraPortalB2B.Partners.App.Abstractions.System;

namespace AuroraPortalB2B.Partners.Infrastructure.DependencyInjection;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid New() => Guid.NewGuid();
}
