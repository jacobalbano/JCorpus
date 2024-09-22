using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokuroWrapper;

[JsonConverter(typeof(CoordConverter))]
public record class MokuroCoord(
    double X,
    double Y
)
{
    public override string ToString() => $"{X}, {Y}";
}

internal class CoordConverter : JsonConverter<MokuroCoord>
{
    public static CoordConverter Instance { get; } = new();

    public override MokuroCoord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);

        var values = new double[2];
        for (int i = 0; i < values.Length; i++)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            values[i] = reader.GetDouble();
        }

        var result = new MokuroCoord(values[0], values[1]);
        Debug.Assert(reader.Read());
        Debug.Assert(reader.TokenType == JsonTokenType.EndArray);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, MokuroCoord value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}