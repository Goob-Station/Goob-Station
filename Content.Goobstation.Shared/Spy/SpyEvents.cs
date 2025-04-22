using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Serializable, NetSerializable]
public sealed partial class SpyStealDoAfterEvent : DoAfterEvent
{
    public NetEntity Sound;

    public SpyStealDoAfterEvent(NetEntity sound)
    {
        Sound = sound;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed class SpyStartStealEvent(NetEntity target) : EntityEventArgs
{
    public NetEntity Target = target;
}
