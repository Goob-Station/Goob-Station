namespace Content.Goobstation.Common.Medical;

/// <summary>
/// Checks if the entity should take damage on limb amputations.
/// </summary>
[ByRefEvent]
public record struct BeforeAmputationDamageEvent(
    bool Cancelled = false);
