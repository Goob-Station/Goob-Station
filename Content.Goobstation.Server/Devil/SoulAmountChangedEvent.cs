namespace Content.Goobstation.Server.Devil;

/// <summary>
/// Raised on a devil when the amount of souls in their storage changes.
/// </summary>
/// <param name="user">The Devil gaining souls.</param>
/// <param name="victim">The entity losing its soul.</param>
/// <param name="amount">How many souls they are gaining.</param>
[ByRefEvent]
public record struct SoulAmountChangedEvent(EntityUid user, EntityUid victim, int amount);
