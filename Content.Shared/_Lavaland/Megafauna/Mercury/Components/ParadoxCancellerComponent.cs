using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// On use in hand, save current position and current health values, then start a timer.
/// Once the timer finishes, return to saved position and return to whatever health values were on use of item.
/// Will techically "hurt" you if you healed during the rewind timer.
/// Ideally also reverts burning, crushed bones and bleeding, but these systems aren't predicted, and frankly a bit of a hassle to do.
/// </summary>
[RegisterComponent]
public sealed partial class ParadoxCancellerComponent : Component
{
    [DataField]
    public EntityUid? HeldBy;

    /// <summary>
    /// How long before the rewind kicks in.
    /// </summary>
    [DataField]
    public float RewindTime = 5f;

    /// <summary>
    /// Rewing trigger deadline.
    /// </summary>
    [DataField]
    public TimeSpan? RewindDeadline;

    /// <summary>
    /// Coordinate to return to.
    /// </summary>
    [DataField]
    public EntityCoordinates? SavedPosition;

    /// <summary>
    /// Health values to return to.
    /// </summary>
    [DataField]
    public DamageSpecifier? SavedDamage;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/communicating.ogg");

    [DataField]
    public SoundSpecifier RewindSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");

    [DataField]
    public ComponentRegistry? Trail;

    /// <summary>
    /// Spawn this on use to keep track of position to return to.
    /// </summary>
    [DataField]
    public EntProtoId? MarkerPrototype = "EffectParadox";
    public EntityUid? MarkerEntity;
}
