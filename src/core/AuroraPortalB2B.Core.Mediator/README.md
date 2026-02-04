# AuroraPortalB2B.Core.Mediator

A minimal mediator implementation with request/handler dispatching and pipeline behaviors.

## Usage

```csharp
services.AddMediator(options =>
{
    options.AddAssemblies(AssemblyInfo.Assembly);
    // options.AddPipelineBehavior(typeof(IPipelineBehavior<,>), typeof(MyBehavior<,>));
});
```

## Example

```csharp
public record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}
```
