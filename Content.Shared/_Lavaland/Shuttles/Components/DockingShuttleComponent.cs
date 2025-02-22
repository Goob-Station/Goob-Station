using Content.Shared._Lavaland.Shuttles.Systems;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Shuttles.Components;

/// <summary>
/// Component that stores destinations a docking-only shuttle can use.
/// Used by <see cref="DockingConsoleComponent"/> to access destinations.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedDockingShuttleSystem))]
public sealed partial class DockingShuttleComponent : Component
{
    /// <summary>
    /// The station this shuttle belongs to.
    /// </summary>
    [DataField]
    public EntityUid? Station;

    /// <summary>
    /// Every destination this console can FTL to.
    /// </summary>
    [DataField]
    public List<DockingDestination> Destinations = new();

    /// <summary>
    /// Airlock tag that it will prioritize docking to.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype> DockTag = "DockMining";
}

/// <summary>
/// A map a shuttle can FTL to.
/// Created automatically on shuttle mapinit.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial struct DockingDestination
{
    /// <summary>
    /// The name of the destination to use in UI.
    /// </summary>
    [DataField]
    public LocId Name;

    /// <summary>
    /// The map ID.
    /// </summary>
    [DataField]
    public MapId Map;
}
