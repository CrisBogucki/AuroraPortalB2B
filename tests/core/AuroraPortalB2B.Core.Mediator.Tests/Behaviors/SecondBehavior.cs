using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Tests.Requests;
using AuroraPortalB2B.Core.Mediator.Tests.Helper;

namespace AuroraPortalB2B.Core.Mediator.Tests.Behaviors;

public sealed class SecondBehavior : IPipelineBehavior<Ping, string>
{
    public async Task<string> Handle(Ping request, CancellationToken cancellationToken, Func<Task<string>> next)
    {
        ExecutionLog.Add("before-second");
        var response = await next();
        ExecutionLog.Add("after-second");
        return response;
    }
}
