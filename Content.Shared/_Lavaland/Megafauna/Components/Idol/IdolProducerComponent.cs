using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components.Idol;

/// <summary>
/// The Producer boss component. Holds all configuration data and runtime state
/// for the Producer megafauna's three idol-spawning phases.
///
/// Phase 1 – Single: one idol alive at a time, chosen without repeating until the
///           pool is exhausted, then the shuffle resets.
/// Phase 2 – Group:  one named group alive at a time; a new group only spawns once
///           every member of the previous group is dead.
/// Phase 3 – Replenish: all idols in the pool are kept alive simultaneously;
///           each dead idol is individually respawned after a short cooldown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IdolProducerComponent : Component
{
    /// <summary>
    /// The full ordered roster of individual idol prototypes.
    /// Used as the draw pool for phase 1 (one at a time, no repeats until reset)
    /// and as the "all-alive" target list for phase 3.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> SingleIdolPool = new();

    /// <summary>
    /// Named, hand-authored idol groups for phase 2.
    /// A random group is chosen; once every member is dead the next group can fire.
    /// Each inner list is the set of prototypes spawned together.
    /// </summary>
    [DataField(required: true)]
    public List<List<EntProtoId>> IdolGroupPool = new();

    /// <summary>
    /// Seconds the Producer waits before respawning a dead idol during phase 3.
    /// </summary>
    [DataField]
    public float ReplenishCooldown = 5f;

    /// <summary>
    /// Distance in tiles from the Producer at which idols are spawned.
    /// </summary>
    [DataField]
    public float SpawnOffset = 1.5f;

    /// <summary>
    /// Indices into <see cref="SingleIdolPool"/> that have not yet been drawn
    /// in the current shuffle cycle. Reset (refilled) when empty.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<int> Phase1RemainingIndices = new();

    /// <summary>
    /// The single idol currently alive on the field during phase 1.
    /// Null when no idol is alive (i.e. the next summon is permitted).
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Phase1LiveIdol = null;

    /// <summary>
    /// All entities belonging to the group that was most recently spawned.
    /// A new group may only be spawned once this list is entirely dead/empty.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> Phase2LiveGroup = new();

    /// <summary>
    /// Per-prototype respawn timer, keyed by position in <see cref="SingleIdolPool"/>.
    /// When an idol dies its index is added here with a timestamp of
    /// <c>GameTiming.CurTime + ReplenishCooldown</c>.
    /// The system polls this every update and spawns when the time is reached.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<int, TimeSpan> Phase3RespawnTimers = new();

    /// <summary>
    /// The currently alive idol entities for phase 3, keyed by pool index.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<int, EntityUid> Phase3LiveIdols = new();

    [DataField]
    public EntProtoId? SingleSpawnAction;

    [DataField]
    public EntProtoId? GroupSpawnAction;

    [DataField]
    public EntProtoId? ReplenishSpawnAction;

    [DataField, AutoNetworkedField]
    public EntityUid? SingleSpawnActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? GroupSpawnActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ReplenishSpawnActionEntity;
}
