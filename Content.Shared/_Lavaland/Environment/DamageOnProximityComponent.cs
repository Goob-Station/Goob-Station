using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Shared._Lavaland.Environment;

/// <summary>
/// Periodically damages entities within range.
/// </summary>
[RegisterComponent]
public sealed partial class DamageOnProximityComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public float Interval = 1.0f;

    [DataField]
    public float Range = 1.0f;

    [DataField]
    public TimeSpan NextDamageTime;

    /// <summary>
    /// Cooldowns are shared between damage groups.
    /// </summary>
    [DataField]
    public string Group = string.Empty;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Sound that plays when damage is dealt. You REALLY shouldn't put a sound if the damage interval is short.
    /// </summary>
    [DataField]
    public SoundPathSpecifier? DamageSound;
}
