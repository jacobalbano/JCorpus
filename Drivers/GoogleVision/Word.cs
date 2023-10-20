using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVision;

public record class Word(
    float Confidence,
    BoundingBox BoundingBox,
    IReadOnlyList<Symbol> Symbols
)
{
    internal static Word From(Google.Cloud.Vision.V1.Word x) =>  new(
        x.Confidence,
        BoundingBox.From(x.BoundingBox),
        x.Symbols.Select(Symbol.From).ToList()
    );

    public override string ToString() => string.Join("", Symbols);
}
