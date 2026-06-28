using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Banana.Components;

/// <summary>
/// Component that handles creating a flurry of projectiles around the entity in random directions.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaProjectileFlurryComponent : Component
{
    [DataField]
    public int ProjectileNumber;

    [DataField]
    public float Speed;

    [DataField]
    public float SpawnDelay;

    [DataField]
    public bool ShouldSpeak;

    [DataField]
    public LocId Speech = "childish-oni-flurry";

    /// <summary>
    /// The projectile prototype fired in random directions.
    /// </summary>
    [DataField]
    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId Prototype = "FlamingSlashProjectileTemporary";
}
