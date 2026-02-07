using AuroraPortalB2B.Partners.App.Common;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.App.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldReturnFailureWithError()
    {
        var result = Result.Fail("ERR", "Something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ERR");
        result.Error!.Message.Should().Be("Something went wrong");
    }
}
