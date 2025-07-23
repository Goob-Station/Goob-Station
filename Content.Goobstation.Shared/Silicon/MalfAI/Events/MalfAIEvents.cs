using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Events;

[Serializable, NetSerializable]
public sealed partial class HackDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public readonly struct OnHackedEvent(Entity<MalfStationAIComponent> hacker)
{
    public readonly Entity<MalfStationAIComponent> HackerEntity = hacker;
};

[ByRefEvent]
public struct OnAboutToUseCostlyAbility(int cost)
{
    public readonly int Cost = cost;

    public bool Cancelled = false;
}

public sealed partial class MalfAIOpenShopAction : InstantActionEvent;

public sealed partial class MachineOverloadActionEvent : EntityTargetActionEvent;