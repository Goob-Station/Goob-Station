using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Doodon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PapaDoodonComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool TownhallPlaced = false;
}


public sealed partial class EstablishTownhallTileActionEvent : WorldTargetActionEvent
{
    public EstablishTownhallTileActionEvent(EntityUid performer, EntityCoordinates target) : this()
    {
        Performer = performer;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed partial class EstablishTownhallDoAfterEvent : SimpleDoAfterEvent
{
    [NonSerialized] public EntityCoordinates Coords;
    public EstablishTownhallDoAfterEvent(EntityCoordinates coords)
    {
        Coords = coords;
    }
}
