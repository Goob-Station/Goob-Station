using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent]
public sealed partial class BlobResourceComponent : Component
{
    [DataField]
    public FixedPoint2 PointsPerPulsed = 3;
}
