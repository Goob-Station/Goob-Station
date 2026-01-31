using Content.Shared.Actions;

namespace Content.Goobstation.Shared.ImmortalSnail;

/// <summary>
/// Event raised when the immortal snail uses Touch of Death on a target.
/// </summary>
public sealed partial class TouchOfDeathEvent : EntityTargetActionEvent
{
}

/// <summary>
/// Event raised when the immortal snail uses Track Target to sense their target.
/// </summary>
public sealed partial class SnailHeartbeatEvent : InstantActionEvent
{
}

/// <summary>
/// Event raised when the immortal snail's target is set.
/// </summary>
[ByRefEvent]
public readonly record struct ImmortalSnailTargetSetEvent();

/// <summary>
/// Event raised when the immortal snail uses Touch of Death on its target.
/// </summary>
public sealed partial class TouchOfDeathEvent : EntityTargetActionEvent
{
}
