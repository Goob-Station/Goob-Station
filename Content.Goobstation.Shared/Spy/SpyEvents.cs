using Content.Shared.DoAfter;
using Content.Shared.Objectives;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Serializable, NetSerializable]
public sealed partial class SpyStealDoAfterEvent : DoAfterEvent
{
    public NetEntity Sound;
    public ProtoId<StealTargetGroupPrototype> StealGroup;

    public SpyStealDoAfterEvent(NetEntity sound, ProtoId<StealTargetGroupPrototype> stealGroup)
    {
        Sound = sound;
        StealGroup = stealGroup;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class UplinkCreateDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class SpyStartStealEvent(NetEntity target) : EntityEventArgs
{
    public NetEntity Target = target;
}
