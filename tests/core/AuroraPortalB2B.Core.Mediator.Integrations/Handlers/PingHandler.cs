using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Integrations.Requests;

namespace AuroraPortalB2B.Core.Mediator.Integrations.Handlers;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong:{request.Message}");
}
