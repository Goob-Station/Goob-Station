using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Doodon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class DoodonComponent : Component
{
    /// <summary>
    /// The townhall that this doodon is linked to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid LinkedTownhall = default!;
}

# region Events
public sealed partial class BuildDoodonStructureActionEvent : WorldTargetActionEvent
{
    public BuildDoodonStructureActionEvent(EntityUid performer, EntityCoordinates target) : this()
    {
        Performer = performer;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed partial class BuildDoodonStructureDoAfterEvent : SimpleDoAfterEvent
{
    /// <summary>
    /// Where the doodon structure is being placed.
    /// </summary>
    [NonSerialized] public EntityCoordinates Coords;
    public BuildDoodonStructureDoAfterEvent(EntityCoordinates coords)
    {
        Coords = coords;
    }
}

public sealed partial class DoodonStructureCycleEvent : InstantActionEvent { }
#endregion
