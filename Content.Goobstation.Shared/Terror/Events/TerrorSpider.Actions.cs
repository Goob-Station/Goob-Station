using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Terror.Events;

public sealed partial class TerrorWebEvent : InstantActionEvent;

public sealed partial class TerrorWrapEvent : EntityTargetActionEvent;

public sealed partial class TerrorLayEggEvent : InstantActionEvent;

public sealed partial class TerrorLayEggGreenEvent : EntityTargetActionEvent;


[Serializable, NetSerializable]
public sealed partial class TerrorWebDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class TerrorWrapDoAfterEvent : SimpleDoAfterEvent;
