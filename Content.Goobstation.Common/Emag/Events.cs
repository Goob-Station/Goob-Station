namespace Content.Goobstation.Common.Emag;
/// <summary>
/// Raised on the entity when it's emag is cleaned 
/// </summary>
/// <param name="User">The entity</param>
/// <param name="Handled">If event is handled by previous system</param>
[ByRefEvent]
public record struct EmagCleanedEvent(EntityUid User, bool Handled = false);
