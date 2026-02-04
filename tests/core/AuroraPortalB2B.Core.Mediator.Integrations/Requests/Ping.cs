using AuroraPortalB2B.Core.Mediator;

namespace AuroraPortalB2B.Core.Mediator.Integrations.Requests;

public sealed record Ping(string Message) : IRequest<string>;
