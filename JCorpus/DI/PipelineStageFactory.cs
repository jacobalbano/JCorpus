using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Utility;

namespace JCorpus.DI;

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class PipelineStageFactory
{
    public PipelineStageFactory(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public T Create<T>(ConfigurableStage<T> config)
    {
        var repository = provider.GetRequiredService<ITypeRepository<T>>();
        var configRepository = provider.GetRequiredService<ConfigRepository>();
        if (!repository.TryGetTypeByName(config.TypeName, out var implementationType))
            throw new KeyNotFoundException(nameof(config.TypeName));

        if (IsConfigurable(implementationType, out var configType))
            configRepository.Provide(ParseConfig(configType, config.ConfigurationJson));

        return (T)provider.GetRequiredService(implementationType);
    }

    private bool IsConfigurable(Type type, [MaybeNullWhen(false)] out Type configType)
    {
        configType = type.GetInterfaces()
            .Where(x => x.IsGenericType)
            .Where(x => x.GetGenericTypeDefinition() == IConfigurableOpen)
            .Select(x => x.GetGenericArguments().FirstOrDefault())
            .FirstOrDefault();

        return configType != null;
    }

    private static object? ParseConfig(Type configType, JsonDocument? configurationJson)
    {
        if (configurationJson == null)
            return null;

        return JsonSerializer.Deserialize(configurationJson, configType, options);
    }

    private readonly IServiceProvider provider;
    private static readonly Type IConfigurableOpen = typeof(IConfigurable<>);
    private static readonly JsonSerializerOptions options = new() { TypeInfoResolver = new CustomResolver() };

    private class CustomResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = base.GetTypeInfo(type, options);
            foreach (var prop in typeInfo.Properties.Where(x => x.PropertyType == typeof(IVirtualFs)))
                prop.CustomConverter = StringToVfsConverter.Instance;

            return typeInfo;
        }
    }

    private class StringToVfsConverter : JsonConverter<IVirtualFs>
    {
        public static readonly StringToVfsConverter Instance = new();

        public override IVirtualFs? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new VirtualFs(WindowsPathUtility.MakeDirectoryPath(reader.GetString()));
        }

        public override void Write(Utf8JsonWriter writer, IVirtualFs value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ContainingDirectory.ToString());
        }
    }
}
