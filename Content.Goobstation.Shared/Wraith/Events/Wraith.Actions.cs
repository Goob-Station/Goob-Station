using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Events;

// Here belong all action events for the base wraith
public sealed partial class RevenantHauntActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class RevenantHauntWitnessEvent : EntityEventArgs
{
    public HashSet<NetEntity> Witnesses = new();

    public RevenantHauntWitnessEvent(HashSet<NetEntity> witnesses)
    {
        Witnesses = witnesses;
    }

    public RevenantHauntWitnessEvent() : this(new())
    {
    }
}
