using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVision;

public record class Symbol(
    float Confidence,
    BoundingBox BoundingBox,
    string Text
)
{
    internal static Symbol From(Google.Cloud.Vision.V1.Symbol x) => new(
        x.Confidence,
        BoundingBox.From(x.BoundingBox),
        x.Text
    );

    public override string ToString() => Text;
}
