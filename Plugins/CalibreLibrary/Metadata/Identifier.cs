using Common.Configuration;
using Common.Configuration.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalibreLibrary.Metadata;

[JsonConverter(typeof(IdentifierConverter))]
[SchemaDescribe(ConverterType = typeof(IdentifierDescripter))]
public record class Identifier(
    string Scheme,
    string Id
)
{
    private class IdentifierDescripter : ISchemaDescriptor
    {
        public SchemaTypeDefinition GetDefinition() => new IdentifierTypeDescriptor();

        public record class IdentifierTypeDescriptor(
            [property: JsonPropertyName("$format")]
            string Format = "<scheme>:<id>"
        ) : SchemaTypeDefinition("string");
    }

    private class IdentifierConverter : JsonConverter<Identifier>
    {
        public JsonTypename Typename => JsonTypename.String;

        public override Identifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Identifier value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }

    public override string ToString() => $"{Scheme}:{Id}";

    public static Identifier Parse(string str)
    {
        var colon = str.IndexOf(':');
        if (colon <= 0)
            throw new Exception($"Invalid identifier format '{str}'");

        return new(str[..colon], str[(colon + 1)..]);
    }

    public static readonly string Wildcard = "*";
}
