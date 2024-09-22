using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokuroWrapper;

[JsonConverter(typeof(LineConverter))]
public record class MokuroLine(
    MokuroCoord TopLeft,
    MokuroCoord TopRight,
    MokuroCoord BottomRight,
    MokuroCoord BottomLeft
);

internal class LineConverter : JsonConverter<MokuroLine>
{
    public static LineConverter Instance { get; } = new();

    public override MokuroLine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);
        Debug.Assert(reader.Read());

        var values = new MokuroCoord[4];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = CoordConverter.Instance.Read(ref reader, typeToConvert, options)!;
            Debug.Assert(reader.Read());
        }

        var result = new MokuroLine(values[0], values[1], values[2], values[3]);
        Debug.Assert(reader.TokenType == JsonTokenType.EndArray);
        reader.Skip();
        return result;
    }

    public override void Write(Utf8JsonWriter writer, MokuroLine value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        CoordConverter.Instance.Write(writer, value.TopLeft, options);
        CoordConverter.Instance.Write(writer, value.TopRight, options);
        CoordConverter.Instance.Write(writer, value.BottomRight, options);
        CoordConverter.Instance.Write(writer, value.BottomLeft, options);
        writer.WriteEndArray();
    }
}