using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Banana;

/// <summary>
/// Component that handles spawning an entity in any of the four cardinal directions of the target with an offset.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MegafaunaDirectionalSpawnComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId GoRightPrototype = "BananaOniHandLeft";

    [ValidatePrototypeId<EntityPrototype>]
    public EntProtoId GoLeftPrototype = "BananaOniHandRight";

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
