using System.Reflection;
using AuroraPortalB2B.Core.Mediator.Options;

namespace AuroraPortalB2B.Core.Mediator.Extensions;

public static class MediatorOptionsExtensions
{
    public static MediatorOptions AddAssemblies(this MediatorOptions options, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            options.Assemblies.Add(assembly);
        }
        return options;
    }

    public static MediatorOptions AddPipelineBehavior(
        this MediatorOptions options,
        Type serviceType,
        Type implementationType)
    {
        options.PipelineBehaviors.Add((serviceType, implementationType));
        return options;
    }

}
