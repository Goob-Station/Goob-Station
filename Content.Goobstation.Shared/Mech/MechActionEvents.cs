using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Mech;

public sealed partial class MechOverclockActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class MechTurnDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class MechFaceRequestEvent(NetEntity mech, Direction direction) : EntityEventArgs
{
    public NetEntity Mech = mech;
    public Direction Direction = direction;
}
