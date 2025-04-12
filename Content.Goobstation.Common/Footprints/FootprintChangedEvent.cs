using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Footprints;

[Serializable, NetSerializable]
public sealed class FootprintChangedEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}
