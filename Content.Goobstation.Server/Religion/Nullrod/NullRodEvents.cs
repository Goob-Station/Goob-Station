namespace Content.Goobstation.Server.Religion.Nullrod;

/// <summary>
/// 	Raised on the nullrod when praying.
/// </summary>
/// <param name="User">The entity praying at the nullrod.</param>
/// <param name="NullRodComponent">The nullrod comp of the nullrod being prayed at.</param>
/// <param name="Nullrod">The nullrod being prayed at.</param>
[ByRefEvent]
public record struct NullrodPrayEvent(EntityUid User, NullrodComponent NullRodComponent, EntityUid? Nullrod);
