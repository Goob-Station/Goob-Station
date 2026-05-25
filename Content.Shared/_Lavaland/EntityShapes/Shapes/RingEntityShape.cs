using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

public sealed partial class RingEntityShape : EntityShape
{
    // Radius of ring.
    [DataField]
    public float Radius = 2f;

    // Offset in degrees.
    [DataField]
    public float StartAngleDegrees;

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        var result = new List<Vector2>(Size);
        if (Size <= 0)
            return result;

        var step = 360f / Size;
        for (var i = 0; i < Size; i++)
        {
            var deg = StartAngleDegrees + step * i;
            var rad = MathHelper.DegreesToRadians(deg);
            result.Add(Offset + new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * Radius);
        }

        return result;
    }
}
