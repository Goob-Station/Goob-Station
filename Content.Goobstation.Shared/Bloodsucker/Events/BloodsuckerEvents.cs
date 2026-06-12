namespace Content.Goobstation.Shared.Bloodsuckers.Events;

/// <summary>
/// Raised on a bloodsucker when a flash effect is being applied.
/// </summary>
[ByRefEvent]
public record struct BloodsuckerFlashedEvent(float OriginalDuration)
{
    public float ModifiedDuration = OriginalDuration;
}

/// <summary>
/// Raised before any burn-damage healing is applied to a bloodsucker.
/// </summary>
[ByRefEvent]
public record struct BloodsuckerBurnHealAttemptEvent(bool Cancelled = false);

/// <summary>
/// Broadcast on the bloodsucker entity whenever its humanity value changes.
/// </summary>
[ByRefEvent]
public record struct BloodsuckerHumanityChangedEvent(float OldHumanity, float NewHumanity);

/// <summary>
/// Raised when a bloodsucker drops below the gated-action humanity threshold.
/// </summary>
public sealed class BloodsuckerActionsGatedEvent : EntityEventArgs;

/// <summary>
/// Raised when a bloodsucker rises back above the gated-action threshold.
/// </summary>
public sealed class BloodsuckerActionsRestoredEvent : EntityEventArgs;

/// <summary>
/// Raised on the bloodsucker entity the moment it enters a frenzy.
/// </summary>
public sealed class BloodsuckerFrenzyEnteredEvent : EntityEventArgs
{
    public float BurnDamagePerSecond;
}

/// <summary>
/// Raised on the bloodsucker entity the moment it exits a frenzy.
/// </summary>
public sealed class BloodsuckerFrenzyExitedEvent : EntityEventArgs;
