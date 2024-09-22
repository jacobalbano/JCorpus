using Common.Addins;
using Common.Configuration;
using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Jobs;
using JCorpus.Utility;
using JCorpus.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Utility;
using Utility.IO;

namespace JCorpus.DI;

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class ObjectConfigurator
{
    public ObjectConfigurator(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public T Create<T>(ObjectConfiguration<T> config) where T : IAddin
    {
        var repository = provider.GetRequiredService<IAddinRepository<T>>();
        var key = new AddinKey(config.PluginName, config.AddinName);
        if (!repository.TryGetTypeByKey(key, out var implementationType))
            throw new KeyNotFoundException(key.ToString());

        var result = (T)provider.GetRequiredService(implementationType);
        if (IsConfigurable(implementationType, out var configType))
            ((IConfigurable)result).Configure(ParseConfig(configType, config.ConfigurationJson));

        return result;
    }

    public static bool IsConfigurable(Type type, out Type configType)
    {
        configType = type.GetInterfaces()
            .Where(x => x.IsGenericType)
            .Where(x => x.GetGenericTypeDefinition() == IConfigurableOpen)
            .Select(x => x.GetGenericArguments().FirstOrDefault())
            .FirstOrDefault();

        return configType != null;
    }

    private static object ParseConfig(Type configType, JsonDocument configurationJson)
    {
        if (configurationJson == null)
            return null;

        return JsonSerializer.Deserialize(configurationJson, configType, options);
    }

    private readonly IServiceProvider provider;
    private static readonly Type IConfigurableOpen = typeof(IConfigurableWith<>);
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
}
