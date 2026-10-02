using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Counter;

/// <summary>
/// Placed on an action entity (alongside e.g. an InstantAction).
/// Using the action adds the counter component to the user,
/// after it's armed if someone hits them the damage gets negated and it will send out an event for the user.
/// </summary>
[RegisterComponent]
public sealed partial class CounterActionComponent : Component
{
    /// <summary>
    /// How long the counter spends arming (and vulnerable) before it becomes active.
    /// </summary>
    [DataField]
    public TimeSpan BuildupTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long the counter stays armed once built up.
    /// </summary>
    [DataField]
    public TimeSpan CounterTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// If true a countered projectile passes through the user.
    /// If false the countered projectile stops at the user and still negates the damage.
    /// </summary>
    [DataField]
    public bool DodgeProjectile = true;

    /// <summary>
    /// Only projectile hits trigger the counter.
    /// </summary>
    [DataField]
    public bool ProjectileOnly;

    /// <summary>
    /// Only melee hits trigger the counter.
    /// </summary>
    [DataField]
    public bool MeleeOnly;

    /// <summary>
    /// Getting hit during the buildup window cancels the counter before it arms.
    /// </summary>
    [DataField]
    public bool CancelOnBuildup = true;

    /// <summary>
    /// Slowdown applied for as long as they have the counter comp.
    /// </summary>
    [DataField]
    public float SpeedModifier = 0.3f;

    [DataField]
    public string? BuildupShader;

    [DataField]
    public string? ArmedShader;

    [DataField]
    public string? TriggeredShader;

    /// <summary>
    /// How long that flash lasts.
    /// </summary>
    [DataField]
    public TimeSpan TriggerFlashTime = TimeSpan.FromSeconds(0.4);

    /// <summary>
    /// Played on the user as the counter starts building up.
    /// </summary>
    [DataField]
    public SoundSpecifier? BuildupSound;

    /// <summary>
    /// Played on the user the moment the counter arms.
    /// </summary>
    [DataField]
    public SoundSpecifier? ArmedSound;

    /// <summary>
    /// Looped under the whole armed window (give it loop: true); cut the moment the counter fires or ends.
    /// </summary>
    [DataField]
    public SoundSpecifier? ArmedLoopSound;

    /// <summary>
    /// Heartbeat like sound.
    /// </summary>
    [DataField]
    public SoundSpecifier? BeatSound;

    /// <summary>
    /// Played on the user when an armed counter catches a hit.
    /// </summary>
    [DataField]
    public SoundSpecifier? TriggerSound;

    /// <summary>
    /// Played on the user when the counter fizzles: cancelled during buildup, or the armed window expires.
    /// </summary>
    [DataField]
    public SoundSpecifier? FizzleSound;
}
