using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Component that handles creating a flurry of projectiles around the entity in random directions.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaProjectileFlurryComponent : Component
{
    /// <summary>
    /// How many projectiles to spawn before stopping the action.
    /// </summary>
    [DataField]
    public int ProjectileNumber;

    /// <summary>
    /// How quickly the projectiles are shot out.
    /// </summary>
    [DataField]
    public float Speed;

    /// <summary>
    /// Time between spawning each projectile.
    /// </summary>
    [DataField]
    public float SpawnDelay;

    /// <summary>
    /// So that other entities can use this action without using the boss's voiceline.
    /// </summary>
    [DataField]
    public bool ShouldSpeak;

    /// <summary>
    /// What the entity says when using this action.
    /// </summary>
    [DataField]
    public LocId Speech = "childish-oni-flurry";

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId Prototype = "FlamingSlashProjectileTemporary";
}
