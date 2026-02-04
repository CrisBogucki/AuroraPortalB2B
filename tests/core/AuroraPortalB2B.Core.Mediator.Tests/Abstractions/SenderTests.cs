using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Extensions;
using AuroraPortalB2B.Core.Mediator.Tests.Behaviors;
using AuroraPortalB2B.Core.Mediator.Tests.Requests;
using AuroraPortalB2B.Core.Mediator.Tests.Helper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Core.Mediator.Tests.Abstractions;

public sealed class SenderTests
{
    [Fact]
    public async Task Send_ShouldInvokePipelineInOrder()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddScoped<IPipelineBehavior<Ping, string>, FirstBehavior>();
        services.AddScoped<IPipelineBehavior<Ping, string>, SecondBehavior>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        ExecutionLog.Reset();

        // act
        var response = await sender.Send(new Ping("Hi"));

        // assert
        response.Should().Be("Pong:Hi");
        ExecutionLog.Items.Should().ContainInOrder(
            "before-first",
            "before-second",
            "handler",
            "after-second",
            "after-first");
    }
}
