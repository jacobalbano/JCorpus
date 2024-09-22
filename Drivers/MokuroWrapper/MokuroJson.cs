using Common.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokuroWrapper;

public record class MokuroJsonFile(
    FilePath FileName,
    MokuroJson Json
);

public record class MokuroJson(
    [property: JsonPropertyName("version")]
    string Version,

    [property: JsonPropertyName("img_width")]
    int ImageWidth,

    [property: JsonPropertyName("img_height")]
    int ImageHeight,

    [property: JsonPropertyName("blocks")]
    IReadOnlyList<MokuroBlock> Blocks
);

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

    public override void Write(Utf8JsonWriter writer, MokuroBoundingBox value, JsonSerializerOptions options) => throw new NotImplementedException();
}

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

    public override void Write(Utf8JsonWriter writer, MokuroLine value, JsonSerializerOptions options) => throw new NotImplementedException();
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

    public override void Write(Utf8JsonWriter writer, MokuroCoord value, JsonSerializerOptions options) => throw new NotImplementedException();
}