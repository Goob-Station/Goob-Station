using System.Numerics;
using Content.Shared.GridPreloader.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Shelter;

[RegisterComponent]
public sealed partial class ShelterCapsuleComponent : Component
{
    [DataField]
    public float DeployTime = 1f;

    [DataField(required: true)]
    public ProtoId<PreloadedGridPrototype> PreloadedGrid;

    [DataField(required: true)]
    public Vector2 BoxSize;

    /// <remarks>
    /// This is needed only to fix the grid. Capsule always should spawn
    /// at the center, and this vector is required to ensure that.
    /// </remarks>>
    [DataField]
    public Vector2 Offset;
}
