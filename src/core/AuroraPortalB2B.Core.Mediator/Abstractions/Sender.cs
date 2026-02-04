using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Core.Mediator.Abstractions;

internal class Sender(IServiceProvider provider) : ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = provider.GetRequiredService(handlerType);

        Func<Task<TResponse>> handlerDelegate = () =>
            ((dynamic)handler).Handle((dynamic)request, cancellationToken);

        var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var behaviors = provider.GetServices(pipelineType).Cast<dynamic>().ToList();

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((dynamic)request, cancellationToken, next);
        }

        return handlerDelegate();
    }
}
