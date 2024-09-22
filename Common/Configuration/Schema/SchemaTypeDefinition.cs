using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Configuration.Schema;

[JsonConverter(typeof(Converter))]
public abstract record class SchemaTypeDefinition(
    [property: JsonPropertyName("$type")]
    string Type
)
{
    private class Converter : JsonConverter<SchemaTypeDefinition>
    {
        public override SchemaTypeDefinition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, SchemaTypeDefinition value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(value, value.GetType(), options));
        }
    }
};