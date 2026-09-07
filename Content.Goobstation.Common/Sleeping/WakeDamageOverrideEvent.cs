namespace Content.Goobstation.Common.Sleeping;

/// <summary>
/// Raised whenever entity that slept are getting damage
/// Solely to prevent automatic wakeup from damage e.g xenobio slime toxin
/// so you can wake up whenever you want
/// </summary>
[ByRefEvent]
public record struct WakeDamageOverrideEvent(bool IgnoreDamage = false, bool Cancelled = false)
{
    /// <summary>
    /// Should this entity ignore the wake up threshold
    /// </summary>
    public bool IgnoreDamage = IgnoreDamage;
}
