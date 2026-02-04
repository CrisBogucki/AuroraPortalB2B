using AuroraPortalB2B.Core.Mediator;
using Microsoft.Extensions.Logging;

namespace AuroraPortalB2B.Partners.App.Common;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}
