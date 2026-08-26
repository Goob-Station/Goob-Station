using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Terror.Events;

#region Action

public sealed partial class TerrorPounceEvent : WorldTargetActionEvent;
public sealed partial class TerrorChargeEvent : WorldTargetActionEvent;
public sealed partial class TerrorTremorEvent : InstantActionEvent;
public sealed partial class TerrorLifeShareEvent : EntityTargetActionEvent;
public sealed partial class TerrorWebEvent : InstantActionEvent;
public sealed partial class TerrorWrapEvent : EntityTargetActionEvent;
public sealed partial class TerrorLayEvent : EntityTargetActionEvent;
public sealed partial class TerrorQueenSenseEvent : InstantActionEvent;

[ByRefEvent]
public sealed partial class TerrorVentSmashEvent : EntityTargetActionEvent;

#endregion

#region DoAfter

[Serializable, NetSerializable]
public sealed partial class TerrorWebDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class TerrorWrapDoAfterEvent : SimpleDoAfterEvent;

#endregion

#region Internal

[ByRefEvent]
public readonly record struct TerrorWrappedCorpseEvent(EntityUid Spider);

public readonly record struct TerrorSpiderDiedEvent(EntityUid Spider);

public sealed partial class TerrorHiveWrappedEvent : EntityEventArgs;

#endregion
