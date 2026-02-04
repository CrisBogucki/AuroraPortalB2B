using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Extensions;
using AuroraPortalB2B.Core.Mediator.Integrations.Behaviors;
using AuroraPortalB2B.Core.Mediator.Integrations.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Core.Mediator.Integrations.Tests;

public sealed class MediatorIntegrationTests
{
    [Fact]
    public async Task Send_ShouldResolveHandlerFromAssembly()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddMediator(options => options.AddAssemblies(typeof(Ping).Assembly));

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // act
        var response = await sender.Send(new Ping("Hello"));

        // assert
        response.Should().Be("Pong:Hello");
    }

    [Fact]
    public async Task Send_ShouldRunPipelineRegisteredViaOptions()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddMediator(options =>
        {
            options.AddAssemblies(typeof(Ping).Assembly);
            options.AddPipelineBehavior(typeof(IPipelineBehavior<Ping, string>), typeof(FirstBehavior));
        });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // act
        var response = await sender.Send(new Ping("Hi"));

        // assert
        response.Should().Be("Pong:Hi");
    }
}
