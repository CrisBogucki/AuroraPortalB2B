namespace AuroraPortalB2B.Partners.App.Abstractions.System;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
