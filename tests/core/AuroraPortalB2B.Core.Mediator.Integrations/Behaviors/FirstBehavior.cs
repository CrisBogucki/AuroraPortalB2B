using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Integrations.Requests;

namespace AuroraPortalB2B.Core.Mediator.Integrations.Behaviors;

public sealed class FirstBehavior : IPipelineBehavior<Ping, string>
{
    public async Task<string> Handle(Ping request, CancellationToken cancellationToken, Func<Task<string>> next)
    {
        var response = await next();
        return response;
    }
}
