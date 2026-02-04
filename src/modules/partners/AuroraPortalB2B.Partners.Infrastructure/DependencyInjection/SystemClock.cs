using AuroraPortalB2B.Partners.App.Abstractions.System;

namespace AuroraPortalB2B.Partners.Infrastructure.DependencyInjection;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
