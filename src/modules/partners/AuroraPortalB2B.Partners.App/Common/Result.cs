namespace AuroraPortalB2B.Partners.App.Common;

public sealed class Result
{
    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Fail(string code, string message) => new(false, new Error(code, message));
}
