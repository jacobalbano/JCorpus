using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalibreLibrary.Metadata;

[JsonConverter(typeof(IdentifierConverter))]
public record class Identifier(
    string Scheme,
    string Id
)
{
    private class IdentifierConverter : JsonConverter<Identifier>
    {
        public override Identifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            var parts = str.Split(':');
            if (parts.Length != 2)
                throw new Exception($"Invalid identifier format '{str}'");

            return new(parts.First(), parts.Last());
        }

        public override void Write(Utf8JsonWriter writer, Identifier value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }

    public override string ToString() => $"{Scheme}:{Id}";

    public static readonly string Wildcard = "*";
}
