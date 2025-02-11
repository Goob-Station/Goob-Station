using System.Numerics;
using Content.Shared.GridPreloader.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Salvage;

[RegisterComponent]
public sealed partial class ShelterCapsuleComponent : Component
{
    [DataField]
    public float DeployTime = 1f;

    [DataField(required: true)]
    public ProtoId<PreloadedGridPrototype> PreloadedGrid;

    [DataField(required: true)]
    public Vector2 BoxSize;

    [DataField]
    public Vector2 Offset;
}
