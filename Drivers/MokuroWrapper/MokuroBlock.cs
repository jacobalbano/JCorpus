using System.Text.Json.Serialization;

namespace MokuroWrapper;

public record class MokuroBlock(

    [property: JsonPropertyName("box")]
    MokuroBoundingBox Box,

    [property: JsonPropertyName("vertical")]
    bool Vertical,

    [property: JsonPropertyName("font_size")]
    float FontSize,

    [property: JsonPropertyName("lines_coords")]
    IReadOnlyList<MokuroLine> LineCoords,

    [property: JsonPropertyName("lines")]
    IReadOnlyList<string> Lines
);
