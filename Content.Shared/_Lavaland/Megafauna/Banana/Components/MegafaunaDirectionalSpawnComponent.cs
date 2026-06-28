using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Banana.Components;

/// <summary>
/// Component that handles spawning an entity to the left or right side of a target, at a configurable offset.
/// Used for single spawns and timed barrages alike.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaDirectionalSpawnComponent : Component
{
    /// <summary>
    /// Prototype to spawn when the roll picks the right side.
    /// </summary>
    [DataField]
    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId RightPrototype = "BananaOniHandLeft";

    /// <summary>
    /// Prototype to spawn when the roll picks the left side.
    /// </summary>
    [DataField]
    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId LeftPrototype = "BananaOniHandRight";

    /// <summary>
    /// Default offset for single attacks.
    /// </summary>
    [DataField]
    public float Offset = 6f;

    /// <summary>
    /// Minimum offset for barrage attacks.
    /// </summary>
    [DataField]
    public float MinOffset = 3f;

    /// <summary>
    /// Maximum offset for barrage attacks.
    /// </summary>
    [DataField]
    public float MaxOffset = 12f;

    /// <summary>
    /// Time between barrage spawns.
    /// </summary>
    [DataField]
    public float BarrageInterval = 0.8f;

    /// <summary>
    /// Total number of hands in a barrage.
    /// </summary>
    [DataField]
    public int BarrageCount = 3;
}
