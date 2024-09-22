using System.Text.Json;
using System.Text.Json.Serialization;

namespace JCorpus.Web.Transit.Schema;

partial class TransitConfigSchema
{
    private class Converter : JsonConverter<TransitConfigSchema>
    {
        public override TransitConfigSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, TransitConfigSchema value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var prop in value.Properties)
            {
                writer.WritePropertyName(prop.Name);
                writer.WriteRawValue(JsonSerializer.Serialize(prop.Type, prop.Type.GetType()));
            }

            if (value.unmapped.Any())
            {
                writer.WritePropertyName("$unmapped");
                writer.WriteStartArray();
                foreach (var x in value.unmapped)
                    writer.WriteStringValue(x);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
