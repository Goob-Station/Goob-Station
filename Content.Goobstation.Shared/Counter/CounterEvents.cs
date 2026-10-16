using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Counter;

public sealed partial class ArmCounterActionEvent : InstantActionEvent;

[ByRefEvent]
public record struct CounterTriggeredEvent(EntityUid Attacker);
