using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Werewolf.Abilities;

public sealed partial class HowlEvent : InstantActionEvent
{
    [DataField] public float ShriekPower = 2.5f;
    [DataField] public int StunDuration = 1;
}
public sealed partial class TransfurmEvent : InstantActionEvent { }
public sealed partial class EventWerewolfOpenStore : InstantActionEvent {}
public sealed partial class EventWerewolfDevour : EntityTargetActionEvent {}
public sealed partial class EventWerewolfChangeType : InstantActionEvent
{
    [DataField] public string WerewolfType;
}
// public sealed partial class AmbushEvent : WorldTargetActionEvent
// {
//     [DataField] public float Distance = 6.5f;
//     [DataField] public float Speed = 9.65f;
// }

// pohui goida
[Serializable, NetSerializable]
public sealed partial class WerewolfDevourDoAfterEvent : SimpleDoAfterEvent { }
