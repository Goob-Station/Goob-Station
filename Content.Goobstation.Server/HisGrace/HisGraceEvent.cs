namespace Content.Goobstation.Server.HisGrace;

/// <summary>
/// Raised on His Grace when the hunger level changes.
/// </summary>
/// <param name="NewState">The new hunger level of His Grace.</param>
[ByRefEvent]
public record struct HisGraceHungerChangedEvent(HisGraceState NewState);

/// <summary>
/// Raised on His Grace when an entity is consumed
/// </summary>
[ByRefEvent]
public record struct HisGraceEntityConsumedEvent();
