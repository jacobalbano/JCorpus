using System.Text.Json.Serialization;

namespace MokuroWrapper;

[JsonConverter(typeof(LineConverter))]
public record class MokuroLine(
    MokuroCoord TopLeft,
    MokuroCoord TopRight,
    MokuroCoord BottomRight,
    MokuroCoord BottomLeft
)
{
};
