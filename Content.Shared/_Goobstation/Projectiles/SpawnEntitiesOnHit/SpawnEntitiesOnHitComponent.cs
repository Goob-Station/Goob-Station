using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Projectiles.SpawnEntitiesOnHit;

[RegisterComponent]
public sealed partial class SpawnEntitiesOnHitComponent : Component
{
    [DataField]
    public EntProtoId Proto;

    [DataField]
    public int Amount;
}
