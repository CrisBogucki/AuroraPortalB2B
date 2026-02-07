using AuroraPortalB2B.Partners.App.Common;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.App.Tests.Common;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_ShouldReturnValue()
    {
        var result = Result<string>.Success("ok");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("ok");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldReturnFailureWithoutValue()
    {
        var result = Result<int>.Fail("ERR", "Failed");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(0);
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ERR");
        result.Error!.Message.Should().Be("Failed");
    }
}
