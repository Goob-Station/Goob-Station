namespace Content.Goobstation.Common.Conversion;

/// <summary>
/// Used to see if the entity can be converted.
/// </summary>
/// <param name="Uid">The entity being converted.</param>
/// <param name="Blocked">Can the entity be converted?.</param>
[ByRefEvent]
public record struct BeforeConversionEvent(EnityUid Uid, bool Blocked = false);
