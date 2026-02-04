using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Tests.Requests;
using AuroraPortalB2B.Core.Mediator.Tests.Helper;

namespace AuroraPortalB2B.Core.Mediator.Tests.Handlers;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        ExecutionLog.Add("handler");
        return Task.FromResult($"Pong:{request.Message}");
    }
}
