using System.Diagnostics.CodeAnalysis;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._Goobstation.CCVar;
using Content.Shared._Goobstation.Combat;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Wieldable.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Combat;




/// <summary>
///     This system is responsible for managing Library of Ruina/Limbus Company-style ranged attack deflection, or "Ranged Parry".
/// </summary>
public sealed class RangedParrySystem : EntitySystem
{
    //// !WARNING ////
    ////    At base level this allows melee weapons to destroy a ranged attack if the melee weapon deals more damage.
    ////    While in Library of Ruina, this works only if your melee "dice roll" has a higher number than the ranged one, we don't have dices
    ////    Nor they will be really necessary.
    ////
    ////    Currently the idea is such:
    ////    Ranged Parry probability is scaled on target speed, summarized damage (excluding structural) and swing speed of the target's melee, and the player's maximum allowed running velocity.
    ////    Successful parries apply stun to target. Failed do nothing, target takes damage from projectile. This makes sure that no-one can stack reflects from standing still with parrying.
    ////
    ////    Melee user will be at maximum advantage at high running speed with a high damage melee.
    ////    Ranged parry will should almost never proc against a bola'd juggernaut suit.
    ////
    ////    Example:
    ////    Projectile deals 16 damage, melee in active hand of target deals 17 damage. Target has no movement penalties
    ////    Since damage on both parts is almost equal, roll a chance to parry heavily weighed towards failing.
    ////    If rolls for the melee user - destroy projectile, nullify damage, **apply stun** to target based on (the absence of) damage difference.
    ////    If rolls against the melee user - hit.
    //// !WARNING ////

    private float _stamDamageValueBase = 11f;
    private float _stamDamageLowerBound = 2f;
    private float _parryRandomThreshold = 22.0f;
    private float _baseMovementSpeed = MovementSpeedModifierComponent.DefaultBaseSprintSpeed;

    private SoundPathSpecifier _parrySound = new ("/Audio/Weapons/block_metal1.ogg");

    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly StaminaSystem _stam = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileComponent, ProjectileParryAttemptEvent>(OnProjectileParryAttempt);

        Subs.CVar(_configuration, GoobCVars.RangedParryStamDmgValueBase, value =>
        {
            _stamDamageValueBase = value;
            _parryRandomThreshold = value * 2;
        },
            true);
        Subs.CVar(_configuration, GoobCVars.RangedParryStamDmgValueLower, value => _stamDamageLowerBound = value, true);
    }

    private void OnProjectileParryAttempt(EntityUid ent, ProjectileComponent comp, ref ProjectileParryAttemptEvent ev)
    {
        var rangedSummation = GetTotalDamage(comp.Damage.DamageDict);
        if (rangedSummation <= 0)
            return;

        if (!TryGetMeleeSummation(ev.Target, out var meleeSummation))
            return;

        var damage = CalculateParryStamDamage(meleeSummation.Value, rangedSummation);

        var speed = _baseMovementSpeed;

        if (TryComp(ev.Target, out MovementSpeedModifierComponent? movement))
            speed = movement.CurrentSprintSpeed;

        if (meleeSummation > rangedSummation && CastDie(damage, speed))
        {
            var auParams = AudioParams.Default;
            auParams.Pitch += damage * 3;

            _popupSystem.PopupEntity($"Parried! Took {damage} damage!", ev.Target);
            _audio.PlayPvs(_parrySound, ev.Target, auParams);

            _stam.TakeStaminaDamage(ev.Target, value: damage);

            ev.Cancelled = true;
        }
    }

    private bool TryGetMeleeSummation(EntityUid target, [NotNullWhen(true)] out float? summation)
    {
        summation = null;

        if (!HasComp<HandsComponent>(target))
            return false;

        var activeItem = _handsSystem.GetActiveItem(target);

        // No active item, can't parry shit!
        if (activeItem == null)
            return false;

        // It's not a melee weapon
        if (!TryComp(activeItem, out MeleeWeaponComponent? meleeComp))
            return false;

        summation = GetTotalDamage(meleeComp.Damage.DamageDict) + CalculateBonusDamage(activeItem);
        return true;
    }

    private float CalculateBonusDamage(EntityUid? activeItem)
    {
        if (TryComp(activeItem, out WieldableComponent? wield) && wield.Wielded &&
            TryComp(activeItem, out IncreaseDamageOnWieldComponent? incrDamage))
        {
            return GetTotalDamage(incrDamage.BonusDamage.DamageDict);
        }

        return 0;
    }

    private int GetTotalDamage(Dictionary<string, FixedPoint2> damage, bool includeStructural = false)
    {
        int dmg = 0;

        foreach (var kvp in damage)
        {
            if (kvp.Key != "Structural" || includeStructural)
                dmg += kvp.Value.Int();
        }

        return dmg;
    }

    // Gain stamina damage inverse proportional to the difference between melee and ranged summation.
    private float CalculateParryStamDamage(float meleeSummation, float rangedSummation)
    {
        var difference = Math.Abs(meleeSummation - rangedSummation);

        var value = _stamDamageValueBase - difference;

        value = Math.Max(0, value);

        if (difference >= 8)
        {
            value = _stamDamageLowerBound;
        }

        return value;
    }

    private bool CastDie(float damage, float speed)
    {
        //                      22          -    12
        var odds = _parryRandomThreshold - damage * 2;
        odds += odds * (speed - _baseMovementSpeed) / _baseMovementSpeed;
        odds = Math.Clamp(odds / _parryRandomThreshold, 0.0f, 1.0f);

        Console.WriteLine($"Odds {odds}, speed modifier {(speed - _baseMovementSpeed) / _baseMovementSpeed}, damage modifier {_parryRandomThreshold - damage * 2}");

        return _rand.Prob(odds);
    }
}
