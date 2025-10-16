using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hastur.Events;

public sealed partial class HasturDevourEvent : EntityTargetActionEvent;

public sealed partial class HasturLashEvent : EntityTargetActionEvent;

public sealed partial class VeilOfTheVoidEvent : InstantActionEvent;

public sealed partial class HasturCloneEvent : InstantActionEvent;

public sealed partial class MassWhisperEvent : InstantActionEvent;

public sealed partial class InsanityAuraEvent : InstantActionEvent;

public sealed partial class OmnipresenceEvent : InstantActionEvent;


[Serializable, NetSerializable]
public sealed partial class HasturDevourDoAfterEvent : SimpleDoAfterEvent;
