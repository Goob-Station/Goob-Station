using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent]
public sealed partial class BlobStorageComponent : Component
{
    [DataField]
    public FixedPoint2 AddTotalStorage = 100;

    [DataField]
    public FixedPoint2 DeleteOnRemove = 60;
}
