using AuroraPortalB2B.Core.Mediator;

namespace AuroraPortalB2B.Core.Mediator.Tests.Requests;

public sealed record Ping(string Message) : IRequest<string>;
