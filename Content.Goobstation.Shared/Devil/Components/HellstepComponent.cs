using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class HellstepComponent : Component
{
    public float LifetimeTimer = 0f;

    public float SpawnTimer = 0f;

    [DataField]
    public float LifetimeDuration = 6f;

    [DataField]
    public float SpawnInterval = 0.3f;

    [DataField]
    public EntProtoId FirePrototype = "HereticFireAA";

    [DataField]
    public EntProtoId LavaPrototype = "FloorLavaEntityTemporary";
}

