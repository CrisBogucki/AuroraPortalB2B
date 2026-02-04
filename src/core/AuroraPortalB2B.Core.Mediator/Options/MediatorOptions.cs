using System.Reflection;

namespace AuroraPortalB2B.Core.Mediator.Options;

public class MediatorOptions
{
    public IList<Assembly> Assemblies { get; } = new List<Assembly>();
    public IList<(Type ServiceType, Type ImplementationType)> PipelineBehaviors { get; }
        = new List<(Type, Type)>();
}
