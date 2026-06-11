using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Emoting;

[Serializable, NetSerializable, ByRefEvent]
public sealed class SpriteOverrideEvent : EntityEventArgs
{
}
