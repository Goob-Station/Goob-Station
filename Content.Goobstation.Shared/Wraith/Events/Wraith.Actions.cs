using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

// Here belong all action events for the base wraith

public sealed partial class HauntEvent : InstantActionEvent
{
}
public sealed partial class WhisperEvent : InstantActionEvent
{
}
public sealed partial class BloodWritingEvent : InstantActionEvent
{
}
public sealed partial class AbsorbCorpseEvent : EntityTargetActionEvent
{
}
[Serializable, NetSerializable]
public sealed partial class AbsorbCorpseDoAfter : SimpleDoAfterEvent
{
}
public sealed partial class SpookEvent : WorldTargetActionEvent
{
}
public sealed partial class DecayEvent : EntityTargetActionEvent
{
}
public sealed partial class WraithCommandEvent : EntityTargetActionEvent
{
}
public sealed partial class AnimateObjectEvent : InstantActionEvent
{
}
public sealed partial class PossessObjectEvent : EntityTargetActionEvent
{
    [DataField] public ComponentRegistry ToAdd = new();
    [DataField] public HashSet<string> ToRemove = new();
}
public sealed partial class WraithEvolveEvent : InstantActionEvent
{
}

// Here belong all action events for the harbinger wraith :clueless:

public sealed partial class RaiseSkeletonEvent : EntityTargetActionEvent
{
}
public sealed partial class SummonPortalEvent : InstantActionEvent
{
}
public sealed partial class SummonVoidCreatureEvent : InstantActionEvent
{
}
public sealed partial class MakeRevenantEvent : EntityTargetActionEvent
{
}
