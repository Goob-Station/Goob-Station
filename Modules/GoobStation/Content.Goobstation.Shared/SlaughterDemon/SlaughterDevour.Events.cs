using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlaughterDemon;

/// <summary>
/// Triggers once an entity devours another entity
/// </summary>
[ByRefEvent]
public record struct SlaughterDevourEvent(
    EntityUid pullingEnt,
    EntityUid pullerEnt);

/// <summary>
///  Raised on the entity that gets devoured
/// </summary>
/// <param name="Devoured"></param>
/// <param name="Devourer"></param>
[ByRefEvent]
public record struct SlaughterDevourAttemptEvent(
    EntityUid Devoured,
    EntityUid Devourer,
    bool Handled = false,
    bool Cancelled = false);

/// <summary>
/// Doafter for when an entity attempts to devour an entity
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlaughterDevourDoAfter : SimpleDoAfterEvent;
