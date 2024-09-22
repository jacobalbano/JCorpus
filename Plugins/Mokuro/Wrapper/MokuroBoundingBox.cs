using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokuroWrapper;

[JsonConverter(typeof(BoxConverter))]
public record class MokuroBoundingBox(MokuroCoord TopLeft, MokuroCoord BottomRight)
{
    public override string ToString() => $"{TopLeft}, {BottomRight}";
}

internal class BoxConverter : JsonConverter<MokuroBoundingBox>
{
    public static BoxConverter Instance { get; } = new();

    public override MokuroBoundingBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);

        var values = new int[4];
        for (int i = 0; i < values.Length; i++)
        {
            Debug.Assert(reader.Read());
            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            values[i] = reader.GetInt32();
        }

        var result = new MokuroBoundingBox(
            new MokuroCoord(values[0], values[1]),
            new MokuroCoord(values[2], values[3])
        );

        Debug.Assert(reader.Read());
        Debug.Assert(reader.TokenType == JsonTokenType.EndArray);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, MokuroBoundingBox value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.TopLeft.X);
        writer.WriteNumberValue(value.TopLeft.Y);
        writer.WriteNumberValue(value.BottomRight.X);
        writer.WriteNumberValue(value.BottomRight.Y);
        writer.WriteEndArray();
    }
}
