using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Deletes any entities within the radius of this entity.
/// Is probably expensive (maybe) so either don't overuse this or don't set that timer too high.
/// </summary>
[RegisterComponent]
public sealed partial class SafeZoneComponent : Component
{
    /// <summary>
    /// Delete these prototypes when within the radius.
    /// </summary>
    [DataField]
    public List<EntProtoId> Blacklist = new();

    /// <summary>
    /// Radius in which to delete entities within.
    /// </summary>
    [DataField]
    public float SafeRadius = 3f;

    /// <summary>
    /// How often to perform the lookup.
    /// </summary>
    [DataField]
    public TimeSpan LookupInterval = TimeSpan.FromSeconds(1); // Should be safe.
    public TimeSpan NextLookupTime;
}
