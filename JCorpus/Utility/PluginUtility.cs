using Common.Addins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Utility;

internal static class PluginUtility
{
    public static IPlugin GetPluginFor<T>()
        => GetPluginFor(typeof(T));

    public static IPlugin GetPluginFor(Type type)
    {
        return cache.GetOrAdd(type.Assembly.GetName(), key =>
        {
            var pluginType = type.Assembly
                .GetExportedTypes()
                .Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == IPluginOpen))
                .FirstOrDefault() ?? throw new Exception($"Failed to find plugin class for {type}");

            var (ctor, args) = GetBestCtor(pluginType);

            return (IPlugin)ctor.Invoke(args);
        });
    }

    private static (ConstructorInfo, object[]) GetBestCtor(Type pluginType)
    {
        foreach (var ctor in pluginType.GetConstructors())
        {
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0) return (ctor, Array.Empty<object>());
            if (parameters.Any(x => !x.HasDefaultValue)) continue;
            return (ctor, parameters.Select(x => x.DefaultValue).ToArray());
        }

        throw new Exception($"No suitable constructor could be found for {pluginType.Name}");
    }

    private static readonly ConcurrentDictionary<AssemblyName, IPlugin> cache = new();
    private readonly static Type IPluginOpen = typeof(IPlugin<>);

    private const BindingFlags Flags = BindingFlags.CreateInstance
        | BindingFlags.Public
        | BindingFlags.Instance
        | BindingFlags.OptionalParamBinding;
}
