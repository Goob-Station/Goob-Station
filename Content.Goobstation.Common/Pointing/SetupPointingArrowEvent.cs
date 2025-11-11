namespace Content.Goobstation.Common.Pointing;

/// <summary>
/// Raised at user when they are pointing at something
/// </summary>
/// <param name="Arrow"></param>
[ByRefEvent]
public record struct SetupPointingArrowEvent(EntityUid Arrow);
