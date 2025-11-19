using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

public sealed partial class RevenantPushEvent : EntityTargetActionEvent;
public sealed partial class TouchOfEvilEvent : InstantActionEvent;
public sealed partial class RevenantShockwaveEvent : InstantActionEvent;
public sealed partial class RevenantCrushEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class RevenantCrushDoAfterEvent : SimpleDoAfterEvent;
