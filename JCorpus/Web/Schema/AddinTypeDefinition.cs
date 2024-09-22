using Common.Configuration;
using Common.Configuration.Schema;
using System.Text.Json.Serialization;

namespace JCorpus.Web.Schema;

[SchemaDescribe]
record class AddinConfigurationTypeDefinition() : SchemaTypeDefinition("object");

[SchemaDescribe]
record class AddinTypeName(
    [property: JsonPropertyName("$addinCategory")]
    string AddinCategory
) : SchemaTypeDefinition("string");

record class AddinTypeDefinition(
    AddinTypeName TypeName,
    AddinConfigurationTypeDefinition ConfigurationJson
) : SchemaTypeDefinition("object")
{
    public class Handler : IPropertyTypeHandler
    {
        bool IPropertyTypeHandler.TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != ObjectConfigurationOpen)
                return false;

            var T = type.GetGenericArguments().First();
            def = new AddinTypeDefinition(new(T.Name), new());
            return true;
        }

        private static readonly Type ObjectConfigurationOpen = typeof(ObjectConfiguration<>);
    }
}
