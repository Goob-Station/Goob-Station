using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class HellstepComponent : Component
{
    public float LifetimeTimer = 0f;
    public float SpawnTimer = 0f;

    [DataField] public float LifetimeDuration = 6f;
    [DataField] public float SpawnInterval = 0.3f;

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId FirePrototype = "HereticFireAA";

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId LavaPrototype = "FloorLavaEntity";
}

