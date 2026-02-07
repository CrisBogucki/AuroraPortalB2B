using AuroraPortalB2B.Partners.App.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Common;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldLogBeforeAndAfter_AndReturnResponse()
    {
        var logger = new Mock<ILogger<LoggingBehavior<DummyRequest, string>>>();
        var behavior = new LoggingBehavior<DummyRequest, string>(logger.Object);
        var request = new DummyRequest();

        var result = await behavior.Handle(request, CancellationToken.None, () => Task.FromResult("ok"));

        result.Should().Be("ok");
        VerifyLogged(logger, "Handling");
        VerifyLogged(logger, "Handled");
    }

    private static void VerifyLogged(Mock<ILogger<LoggingBehavior<DummyRequest, string>>> logger, string contains)
    {
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(contains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public sealed class DummyRequest;
}
