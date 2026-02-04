namespace AuroraPortalB2B.Core.Mediator.Tests.Helper;

public static class ExecutionLog
{
    private static readonly List<string> ItemsInternal = [];

    public static IReadOnlyList<string> Items => ItemsInternal;

    public static void Add(string value) => ItemsInternal.Add(value);

    public static void Reset() => ItemsInternal.Clear();
}
