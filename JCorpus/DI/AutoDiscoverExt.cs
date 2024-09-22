using Common.DI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.DI;

public static class AutoDiscoverExt
{
    public static IServiceCollection RunAutoDiscovery(this IServiceCollection services, IReadOnlyList<Type> types = null)
    {
        types ??= AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetCustomAttribute<AutoDiscoverAssemblyAttribute>() != null)
            .SelectMany(x => x.GetTypes())
            .ToList();

        var includeTypes = types
            .Select(x => (Type: x, Attribute: x.GetCustomAttribute<AutoDiscoverAttribute>(true)))
            .Where(x => x.Attribute != null)
            .ToList();

        foreach (var (type, attribute) in includeTypes)
        {
            var keyType = type;
            var implType = type;
            if (attribute.ImplementationFor != null)
            {
                keyType = attribute.ImplementationFor;
                if (implType.IsGenericTypeDefinition)
                    implType = implType.GetGenericTypeDefinition();
            }

            var options = attribute.Options;
            if (options.HasFlag(AutoDiscoverOptions.ForceInitialize))
                initializeTypes.Add(type);

            var lifetime = options.ToServiceLifetime();
            if (!type.IsAbstract && !type.IsInterface)
                services.Add(new ServiceDescriptor(keyType, implType, lifetime));
            else if (type.IsInterface && options.HasFlag(AutoDiscoverOptions.Implementations))
            {
                var concreteTypes = types.Where(x => type.IsAssignableFrom(x))
                    .Where(x => !x.IsInterface && !x.IsAbstract);

                foreach (var concreteType in concreteTypes)
                {
                    services.Add(new ServiceDescriptor(concreteType, concreteType, lifetime));
                    services.Add(new ServiceDescriptor(keyType, x => x.GetService(concreteType), lifetime));
                }
            }
        }

        return services;
    }

    public static ServiceProvider ForceInitialization(this ServiceProvider serviceProvider)
    {
        foreach (var type in initializeTypes)
            serviceProvider.GetRequiredService(type);
        return serviceProvider;
    }

    public static ServiceLifetime ToServiceLifetime(this AutoDiscoverOptions options)
    {
        if (options.HasFlag(AutoDiscoverOptions.Scoped)) return ServiceLifetime.Scoped;
        if (options.HasFlag(AutoDiscoverOptions.Transient)) return ServiceLifetime.Transient;
        if (options.HasFlag(AutoDiscoverOptions.Singleton)) return ServiceLifetime.Singleton;
        throw new Exception("Unspecified object lifetime");
    }

    private static readonly HashSet<Type> initializeTypes = new();
}