using Common.Configuration;
using Common.Configuration.Schema;
using GenHTTP.Modules.Reflection;
using JCorpus.DI;
using JCorpus.Web.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JCorpus.Web.Transit.Schema;

[JsonConverter(typeof(Converter))]
partial class TransitConfigSchema
{
    public IReadOnlyList<ConfigPropertyDescriptor> Properties => props;

    public IReadOnlyList<string> UnmappedProperties => unmapped;

    public static TransitConfigSchema GetDescriptionIfConfigurable(Type type)
    {
        if (cache.TryGetValue(type, out var result))
            return result;

        if (ObjectConfigurator.IsConfigurable(type, out var configType))
            result = new TransitConfigSchema(configType);

        return cache[type] = result;
    }

    public static bool TryGetTypeDefinition(Type t, out SchemaTypeDefinition typeDefinition)
    {
        return (typeDefinition = handlers.Select(x => (success: x.TryCreateTypeDefinition(t, out var def), def))
            .Where(x => x.success)
            .Select(x => x.def)
            .FirstOrDefault()) != null;
    }

    private TransitConfigSchema(Type type)
    {
        foreach (var prop in type.GetProperties())
        {
            if (TryGetTypeDefinition(prop.PropertyType, out var desc))
                props.Add(new(prop.Name, desc));
            else unmapped.Add(prop.Name);
        }
    }

    private readonly List<ConfigPropertyDescriptor> props = new();
    private readonly List<string> unmapped = new();
    private static readonly Dictionary<Type, TransitConfigSchema> cache = new();
    private static readonly IReadOnlyList<IPropertyTypeHandler> handlers = new IPropertyTypeHandler[]
    {
        new Handler(),
        new AddinTypeDefinition.Handler(),
        new ScalarTypeDefinition.BooleanHandler(),
        new ScalarTypeDefinition.StringHandler(),
        new ScalarTypeDefinition.NumericHandler(),
        new ArrayTypeDefinition.Handler(),
        new EnumTypeDefinition.Handler(),
    };

    private class Handler : IPropertyTypeHandler
    {
        public bool TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (type.GetCustomAttribute<SchemaDescribeAttribute>() is not SchemaDescribeAttribute attr)
                return false;

            if (attr.ConverterType is Type converterType)
            {
                var converter = (ISchemaDescriptor)Activator.CreateInstance(converterType);
                def = converter.GetDefinition();
            }
            else if (attr.Typename != JsonTypename.None)
            {
                def = new ScalarTypeDefinition(attr.Typename.ToString().ToLower());
            }
            else
            {
                def = new ObjectTypeDefinition(new TransitConfigSchema(type));
            }

            return true;
        }
    }
}