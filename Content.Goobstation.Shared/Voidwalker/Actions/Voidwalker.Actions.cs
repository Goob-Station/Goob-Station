using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Voidwalker.Actions;

public sealed partial class VoidwalkerUnsettleEvent : EntityTargetActionEvent;

public sealed partial class VoidWhisperEvent : EntityTargetActionEvent;

public sealed partial class ExitNebulaCrawlEvent : InstantActionEvent;

public sealed partial class VoidwalkerVoidWalkEvent : WorldTargetActionEvent
{
    [DataField]
    public float Distance = 4.65f;

    [DataField]
    public float Speed = 9.65f;
}

[Serializable, NetSerializable]
public sealed partial class VoidwalkerUnsettleDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerKidnapDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerConvertWallDoAfterEvent : SimpleDoAfterEvent;
