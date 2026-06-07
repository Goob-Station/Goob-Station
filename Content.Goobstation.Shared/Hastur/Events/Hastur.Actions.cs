using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hastur.Events;

[ByRefEvent]
public sealed partial class HasturDevourEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class HasturLashEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class VeilOfTheVoidEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class HasturCloneEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class MassWhisperEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class InsanityAuraEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class OmnipresenceEvent : InstantActionEvent;


[Serializable, NetSerializable]
public sealed partial class HasturDevourDoAfterEvent : SimpleDoAfterEvent;
