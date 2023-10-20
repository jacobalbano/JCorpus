using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVision;

public record class Vertex(int X, int Y);

/// <summary>
/// Bounding box represented by vertices.
/// May not be AABB.
/// 
/// </summary>
/// <param name="Vertices"></param>
public record class BoundingBox(IReadOnlyList<Vertex> Vertices)
{
    public Vertex BoundsTopLeft { get; } = new(
        Vertices.Select(vert => vert.X).Min(),
        Vertices.Select(vert => vert.Y).Min()
    );

    public Vertex BoundsBottomRight { get; } = new(
        Vertices.Select(vert => vert.X).Max(),
        Vertices.Select(vert => vert.Y).Max()
    );

    public int Width => BoundsBottomRight.X - BoundsTopLeft.X;
    public int Height => BoundsBottomRight.Y - BoundsTopLeft.Y;

    public static BoundingBox From(Google.Cloud.Vision.V1.BoundingPoly x) => new(
        x.Vertices.Select(vert => new Vertex(vert.X, vert.Y)).ToList()
    );
}
