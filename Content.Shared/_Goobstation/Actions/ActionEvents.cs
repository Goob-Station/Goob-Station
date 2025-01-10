using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Actions;

[Serializable, NetSerializable]
public sealed class LoadActionsEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}
