using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;

/// <summary>
///     Set of <see cref="Content.Shared.Weapons.Melee.MeleeWeaponComponent"/> overrides that get applied
///     to the entity when the matching attack strategy is selected.
///     These are declared per-entity on <see cref="FightActionComponent"/>, because different races have
///     different base attacks (e.g. lizards have a tail attack).
///     Every combat field is nullable: a <c>null</c> value leaves the corresponding field on the
///     weapon untouched, so a profile only has to declare what differs from the current weapon.
/// </summary>
[DataDefinition]
public sealed partial class FightActionMeleeParameters
{
    /// <summary>
    ///     Whether the alt-attack performs a disarm.
    /// </summary>
    [DataField] public bool HasDisarm = false;

    /// <summary>
    ///     Base damage dealt by the weapon. If null the current damage is kept.
    /// </summary>
    [DataField] public DamageSpecifier? Damage;

    /// <summary>
    ///     How many times the weapon can attack per second.
    /// </summary>
    [DataField] public float? AttackRate;

    /// <summary>
    ///     Nearest edge range to hit an entity.
    /// </summary>
    [DataField] public float? Range;

    /// <summary>
    ///     Total width of the angle for wide attacks.
    /// </summary>
    [DataField] public Angle? Angle;

    /// <summary>
    ///     Multiplies damage by this amount for single-target (click) attacks.
    /// </summary>
    [DataField] public FixedPoint2? ClickDamageModifier;

    /// <summary>
    ///     How much of the dealt Blunt damage is also applied to the target as stamina damage.
    ///     Stamina damage = Blunt damage * this factor. Raise it to make hits exhaust targets faster.
    /// </summary>
    [DataField] public FixedPoint2? BluntStaminaDamageFactor;

    /// <summary>
    ///     Stamina cost applied to the attacker on each successful wide swing hit.
    /// </summary>
    [DataField] public float? HeavyStaminaCost;

    /// <summary>
    ///     Whether heavy (wound-up wide) attacks are allowed.
    /// </summary>
    [DataField] public bool? CanHeavyAttack;

    /// <summary>
    ///     Whether wide swings are allowed.
    /// </summary>
    [DataField] public bool? CanWideSwing;

    /// <summary>
    ///     Whether attacks bypass armor resistances.
    /// </summary>
    [DataField] public bool? ResistanceBypass;
}
