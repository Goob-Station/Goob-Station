using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hastur.Events;

/// <summary>
/// This handles actions the raptors can use while being ridden.

public sealed partial class RaptorTargetEvent : EntityTargetActionEvent;

public sealed partial class RaptorInstanEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class RaptorDoAfterEvent : SimpleDoAfterEvent;
