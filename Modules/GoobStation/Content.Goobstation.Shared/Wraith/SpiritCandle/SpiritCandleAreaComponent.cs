using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpiritCandleAreaComponent : Component
{
    /// <summary>
    /// Which entities can collide with the area
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist;

    /// <summary>
    /// Entities currently inside the area.
    /// </summary>
    [ViewVariables]
    public HashSet<EntityUid?> EntitiesInside = new();
}

/// <summary>
/// Raised when attempting to collide with the area
/// </summary>
[ByRefEvent]
public record struct AttemptCollideSpiritCandleEvent(bool Cancelled = false);
