using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVision;

public record class Paragraph(
    float Confidence,
    BoundingBox BoundingBox,
    IReadOnlyList<Word> Words
)
{
    internal static Paragraph From(Google.Cloud.Vision.V1.Paragraph x) => new(
        x.Confidence,
        BoundingBox.From(x.BoundingBox),
        x.Words.Select(x => Word.From(x)).ToList()
    );

    public override string ToString() => string.Join("", Words);
}
