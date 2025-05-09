namespace Content.Goobstation.Common.Construction;

/// <summary>
/// Raised on the user after an entity is created by construction.
/// </summary>
[ByRefEvent]
public readonly record struct ConstructedEvent(EntityUid Entity);
