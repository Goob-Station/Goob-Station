
namespace Content.Server.Psionics;

/// <summary>
///     Raised on an entity about to roll for a Psionic Power, after their baseline chances of success are calculated.
/// </summary>
[ByRefEvent]
public record struct OnRollPsionicsEvent(EntityUid Roller, float BaselineChance);
