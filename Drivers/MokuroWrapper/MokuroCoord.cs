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
