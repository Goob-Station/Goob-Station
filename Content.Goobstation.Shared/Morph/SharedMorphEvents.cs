using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Morph;

public sealed partial class MorphReplicateEvent : InstantActionEvent
{
}

public sealed partial class MorphEvent : EntityTargetActionEvent
{
}

public sealed partial class UnMorphEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ReplicateDoAfterEvent : SimpleDoAfterEvent
{
}

