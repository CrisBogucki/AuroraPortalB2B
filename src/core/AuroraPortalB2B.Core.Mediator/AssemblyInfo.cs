using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AuroraPortalB2B.Core.Mediator.Tests")]

namespace AuroraPortalB2B.Core.Mediator;

public static class AssemblyInfo
{
    public static Assembly Assembly => Assembly.GetExecutingAssembly();
}
