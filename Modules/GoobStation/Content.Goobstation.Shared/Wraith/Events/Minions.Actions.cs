using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

/* Here belong all action events for the wraith minions. */

#region Skeleton Commander

public sealed partial class RallyEvent : InstantActionEvent;

#endregion

#region Void Spiker
public sealed partial class TentacleHookEvent : EntityTargetActionEvent;

public sealed partial class SpikerShuffleEvent : InstantActionEvent;

public sealed partial class SpikerLashEvent : EntityTargetActionEvent;

#endregion

#region Void Hound
public sealed partial class RushdownEvent : InstantActionEvent;

public sealed partial class CloakEvent : InstantActionEvent;

#endregion

#region Plague Rat
public sealed partial class EatFilthEvent : EntityTargetActionEvent;

public sealed partial class RatBiteEvent : EntityTargetActionEvent;

public sealed partial class RatSlamEvent : InstantActionEvent;

public sealed partial class SummonRatDenEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class EatFilthDoAfterEvent : SimpleDoAfterEvent;
#endregion

#region Other
public sealed partial class ChooseVoidCreatureEvent : InstantActionEvent;
#endregion
