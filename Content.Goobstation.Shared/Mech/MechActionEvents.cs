[Serializable, NetSerializable]
public sealed partial class MechTurnDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class MechFaceRequestEvent(NetEntity mech, Direction direction) : EntityEventArgs
{
    public NetEntity Mech = mech;
    public Direction Direction = direction;
}
