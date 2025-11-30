using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Morph;

[ByRefEvent]
public sealed partial class MorphReplicateEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class MorphEvent : EntityTargetActionEvent;

[ByRefEvent]
public sealed partial class UnMorphEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class ReplicateDoAfterEvent : SimpleDoAfterEvent;

