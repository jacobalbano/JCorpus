using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.DI;

[Flags]
public enum AutoDiscoverOptions
{
    None = 0,
    Singleton = 1 << 0,
    Scoped = 1 << 2,
    Transient = 1 << 3,
    Implementations = 1 << 4,
    ForceInitialize = 1 << 5,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public class AutoDiscoverAttribute : Attribute
{
    public AutoDiscoverAttribute(AutoDiscoverOptions options)
    {
        Options = options;
    }

    public Type? ImplementationFor { get; set; }
    public AutoDiscoverOptions Options { get; }
}

public static class AutoDiscoverExtensions
{
    public static IServiceCollection RunAutoDiscovery(this IServiceCollection services, IReadOnlyList<Type> types = null)
    {
        types ??= AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetCustomAttribute<AssemblyProductAttribute>()?.Product.Contains("Microsoft", StringComparison.InvariantCultureIgnoreCase) == false)
            .SelectMany(x => x.GetTypes())
            .ToList();

        var includeTypes = types
            .Select(x => (Type: x, Attribute: x.GetCustomAttribute<AutoDiscoverAttribute>(true)!))
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

            ServiceLifetime lifetime;
            if (options.HasFlag(AutoDiscoverOptions.Scoped))
                lifetime = ServiceLifetime.Scoped;
            else if (options.HasFlag(AutoDiscoverOptions.Transient))
                lifetime = ServiceLifetime.Transient;
            else if (options.HasFlag(AutoDiscoverOptions.Singleton))
                lifetime = ServiceLifetime.Singleton;
            else throw new Exception("Unspecified object lifetime");

            if (!type.IsAbstract && !type.IsInterface)
                services.Add(new ServiceDescriptor(keyType, implType, lifetime));
            else if (type.IsInterface && options.HasFlag(AutoDiscoverOptions.Implementations))
            {
                var concreteTypes = types.Where(x => type.IsAssignableFrom(x))
                    .Where(x => !x.IsInterface)
                    .Where(x => !x.IsAbstract)
                    .ToList();

                foreach (var concreteType in concreteTypes)
                {
                    services.Add(new ServiceDescriptor(keyType, concreteType, lifetime));
                    services.Add(new ServiceDescriptor(concreteType, concreteType, lifetime));
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

    private static readonly HashSet<Type> initializeTypes = new();
}