using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Mech;

public sealed partial class MechOverclockActionEvent : InstantActionEvent;

public sealed partial class MechStompActionEvent : InstantActionEvent;

public sealed partial class MechRamActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class MechTurnDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class MechStompDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class MechFaceRequestEvent(NetEntity mech, Direction direction) : EntityEventArgs
{
    public NetEntity Mech = mech;
    public Direction Direction = direction;
}
