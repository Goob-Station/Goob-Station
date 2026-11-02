using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Damage;

/// <summary>
/// Think back to Minecraft and all shall be clear.
///
///
/// ...Damages entity hitting this component holder. Ranged and melee are processed separately.
/// Damage returned can be a flat amount or a percentage of the damage dealt.
/// Needless to say, percentages can be quite scary so use it sparingly.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThornsComponent : Component
{
    /// <summary>
    /// Reflect damage from melee attacks.
    /// </summary>
    [DataField]
    public bool Melee;

    /// <summary>
    /// Reflect damage from projectiles and hitscan shots, traced back to whoever fired them.
    /// </summary>
    [DataField]
    public bool Ranged;

    /// <summary>
    /// Flat damage dealt to the attacker every time they hurt this entity.
    /// </summary>
    [DataField]
    public DamageSpecifier FlatDamage = new();

    /// <summary>
    /// Fraction of the damage dealt that is returned to the attacker. 0.5 is half.
    /// </summary>
    [DataField]
    public float DamageFraction;

    /// <summary>
    /// Whether the returned damage ignores the attacker's resistances.
    /// </summary>
    [DataField]
    public bool IgnoreResistances;

    /// <summary>
    /// Sound played at the attacker when they take thorns damage.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Popup shown to the attacker when they take thorns damage.
    /// </summary>
    [DataField]
    public LocId PopupText = "thorns-damage-reflected";
}
