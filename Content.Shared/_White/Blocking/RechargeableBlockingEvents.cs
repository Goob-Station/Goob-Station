using Robust.Shared.Serialization;

namespace Content.Shared._White.Blocking;

[Serializable, NetSerializable]
public sealed class ForceTurnOffToggleActiveSound(NetEntity item) : EntityEventArgs
{
    public NetEntity ToggleItem = item;
}
