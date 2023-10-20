using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVision;

public record class TextBlock(
    float Score,
    BoundingBox BoundingBox,
    IReadOnlyList<Paragraph> Paragraphs
)
{
    internal static TextBlock From(Google.Cloud.Vision.V1.Block x)
    {
        return new TextBlock(
            x.Confidence,
            BoundingBox.From(x.BoundingBox),
            x.Paragraphs.Select(x => Paragraph.From(x)).ToList()
        );
    }

    public override string ToString() => string.Join("", Paragraphs);
}
