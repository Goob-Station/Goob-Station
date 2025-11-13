namespace Content.Goobstation.Common.Damage.Events;

/// <summary>
///     Raised on the attacker before stamina damage is applied.
///     Lets offensive traits modify outgoing stamina damage.
/// </summary>
[ByRefEvent]
public record struct ModifyOutgoingStaminaDamageEvent(float Value, EntityUid? Source = null);
