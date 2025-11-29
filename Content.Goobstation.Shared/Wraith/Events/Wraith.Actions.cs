using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;


#region Base Wraith
public sealed partial class HauntEvent : InstantActionEvent;

public sealed partial class WhisperEvent : InstantActionEvent;

public sealed partial class BloodWritingEvent : InstantActionEvent;

public sealed partial class AbsorbCorpseEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class AbsorbCorpseDoAfter : SimpleDoAfterEvent;

public sealed partial class SpookEvent : WorldTargetActionEvent;

public sealed partial class DecayEvent : EntityTargetActionEvent;

public sealed partial class WraithCommandEvent : EntityTargetActionEvent;

public sealed partial class AnimateObjectEvent : InstantActionEvent;

public sealed partial class PossessObjectEvent : EntityTargetActionEvent
{
    [DataField]
    public ComponentRegistry ToAdd = new();

    [DataField]
    public HashSet<string> ToRemove = new();
}

public sealed partial class WraithEvolveEvent : InstantActionEvent;

#endregion

#region Harbinger

public sealed partial class RaiseSkeletonEvent : EntityTargetActionEvent;

public sealed partial class SummonPortalEvent : InstantActionEvent;

public sealed partial class SummonVoidCreatureEvent : InstantActionEvent;

public sealed partial class MakeRevenantEvent : EntityTargetActionEvent;

#endregion

#region Plaguebringer

public sealed partial class BlindCurseEvent : EntityTargetActionEvent;

public sealed partial class WeakCurseEvent : EntityTargetActionEvent;

public sealed partial class BloodCurseEvent : EntityTargetActionEvent;

public sealed partial class RotCurseEvent : EntityTargetActionEvent;

public sealed partial class DeathCurseEvent : EntityTargetActionEvent;

public sealed partial class DefileEvent : EntityTargetActionEvent;

public sealed partial class SummonPlagueRatEvent : InstantActionEvent;

public sealed partial class SummonRotHulkEvent : InstantActionEvent;

#endregion
