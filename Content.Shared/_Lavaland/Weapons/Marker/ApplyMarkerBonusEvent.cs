namespace Content.Shared._Lavaland.Weapons.Marker;

/// <summary>
///     Lavaland Change: We raise this event so that the server can determine how much extra damage to apply to an entity.
/// </summary>
public record struct ApplyMarkerBonusEvent(EntityUid Weapon, EntityUid User);
