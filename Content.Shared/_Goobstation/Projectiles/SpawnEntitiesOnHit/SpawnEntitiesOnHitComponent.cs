using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Projectiles.SpawnEntitiesOnHit;

[RegisterComponent]
public sealed partial class SpawnEntitiesOnHitComponent : Component
{
    /// <summary>
    /// The prototype ID of the entity to spawn on hit
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Proto;

    /// <summary>
    /// The number of entities to spawn when the projectile hits
    /// </summary>
    [DataField]
    public int Amount = 1;
}
