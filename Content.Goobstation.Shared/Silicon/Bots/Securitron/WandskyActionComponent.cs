using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Silicon.Bots.Securitron;

[RegisterComponent, NetworkedComponent]
public sealed partial class CommanderComponent : Component
{
    /// <summary>
    /// What are we using as our waypoint?
    /// </summary>
    [DataField]
    public EntProtoId WaypointEntityUid = "SecuritronWaypoint";

    /// <summary>
    /// A list of waypoints placed.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Waypoints { get; set; } = new();

    /// <summary>
    /// The enslaved robot.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SlaveEntity { get; set; }

    /// <summary>
    /// Which sound to play on enslavement.
    /// </summary>
    [DataField]
    public SoundSpecifier EnslaveSound = new SoundPathSpecifier("/Audio/Machines/chime.ogg");

}

[ByRefEvent]
public sealed partial class TogglePatrolActionEvent : InstantActionEvent;

public sealed partial class WaypointActionEvent : EntityWorldTargetActionEvent;

[ByRefEvent]
public sealed partial class ClearWaypointsActionEvent : InstantActionEvent;

