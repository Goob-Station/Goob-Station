using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Containers;

/// <summary>
/// This is used for a container that can have entities inserted into it via a
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(DragInsertContainerSystem))]
public sealed partial class DragInsertContainerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string ContainerId;

    /// <summary>
    /// If true, there will also be verbs for inserting / removing objects from this container.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool UseVerbs = true;

    /// <summary>
    /// How much time (in seconds) it takes to perform the DoAfter
    /// </summary>
    [DataField("delay")]
    [ViewVariables]
    public float Delay = 0f; //Goobstation
}

[Serializable, NetSerializable, ByRefEvent]
public sealed partial class InsertOnDragDoAfterEvent : SimpleDoAfterEvent
{
}
