using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Hypospray.Events;

public sealed class AfterHyposprayInjectsTargetEvent : HandledEntityEventArgs
{
    public readonly EntityUid User;
    public readonly EntityUid Hypospray;
    public readonly EntityUid Target;

    public AfterHyposprayInjectsTargetEvent(EntityUid user, EntityUid hypospray, EntityUid target)
    {
        User = user;
        Hypospray = hypospray;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed partial class HyposprayTryInjectDoAfterEvent : SimpleDoAfterEvent
{

}
