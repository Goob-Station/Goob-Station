using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Goobstation.Mindshield;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class EffectOnMindshieldedComponent : Component
{
    /// <summary>
    /// Radius of effect if you're actualyl using one. 0 just makes it not have any.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 0f;

    /// <summary>
    /// How often the radius effect is applied.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Damage (or healing if you use negative values) done to folks in the range every time interval runs.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Status effect proto that can be applied if you want.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, TimeSpan> StatusEffects = new();

    /// <summary>
    /// Flat extra damage to deal to mindshielded targets if this is on a weapon.
    /// </summary>
    [DataField]
    public DamageSpecifier MeleeDamage = new();

    /// <summary>
    /// If the effect of this thing can stack with other items of the same type. For AoE healing/damaging you probably really dont want this to stack because powergamers
    /// will powergame.
    /// </summary>
    [DataField]
    public bool Stacks = false;

    /// <summary>
    /// Only stuff of the same group prevents effects from stacking
    /// </summary>
    [DataField]
    public string Group = string.Empty;

    /// <summary>
    /// i wonder
    /// </summary>
    [DataField]
    public bool IgnoreResistances;
}
