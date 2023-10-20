using System.Text.Json.Serialization;

namespace MokuroWrapper;

[JsonConverter(typeof(BoxConverter))]
public record class MokuroBoundingBox(MokuroCoord TopLeft, MokuroCoord BottomRight)
{
    public override string ToString() => $"{TopLeft}, {BottomRight}";
}
