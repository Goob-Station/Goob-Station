using Content.Shared.ActionBlocker;
using Content.Shared.Bed.Sleep;
using Content.Shared.CombatMode;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Goobstation.Execution;

/// <summary>
/// System for make people sleeping without killing them for easier marooning
/// </summary>
public sealed class SharedHitHeadSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _meleeSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitHeadComponent, GetVerbsEvent<UtilityVerb>>(OnGetVerbs);
        SubscribeLocalEvent<HitHeadComponent, HitUnconsciousDoAfterEvent>(OnHitDoAfter);
        SubscribeLocalEvent<HitHeadComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetVerbs(Entity<HitHeadComponent> weapon, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var target = args.Target;

        if (!CanBeHit(target, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartHitDoAfter(weapon, target, attacker),
            Impact = LogImpact.High,
            IconEntity = GetNetEntity(weapon),
            Text = Loc.GetString("hit-head-verb-name"),
            Message = Loc.GetString("hit-head-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private bool TryStartHitDoAfter(Entity<HitHeadComponent> weapon, EntityUid target, EntityUid attacker)
    {
        if (!CanBeHit(target, attacker))
            return false;

        _popupSystem.PopupPredicted(Loc.GetString("hit-head-popup-trying-to-hit", ("attacker", attacker), ("target", target)),
            attacker,
            null,
            PopupType.SmallCaution);

        if (!TryComp<MeleeWeaponComponent>(weapon, out var meleeWeapon))
            return false;

        // Dividng one second to attack rate and multiplying with multiplier to get do after time
        var doAfterTime = (1 / meleeWeapon.AttackRate) * weapon.Comp.DoAfterDurationMultiplier;

        var ev = new HitUnconsciousDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, attacker, doAfterTime, ev, weapon, target, weapon)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return false;

        return true;
    }

    private void OnHitDoAfter(Entity<HitHeadComponent> weapon, ref HitUnconsciousDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        if (!TryComp<MeleeWeaponComponent>(weapon, out var meleeWeaponComp))
            return;

        var attacker = args.User;
        var target = args.Target.Value;
        var comp = weapon.Comp;

        if (!CanBeHit(target, attacker))
            return;

        // Changing attacker combat mode to make hit
        var previousCombatMode = _combatSystem.IsInCombatMode(attacker);
        _combatSystem.SetInCombatMode(attacker, true);
        comp.HitProccess = true;

        if (_meleeSystem.AttemptLightAttack(attacker, weapon, meleeWeaponComp, target))
        {
            // Calculating change to apply sleep
            if (_random.Prob(comp.SleepChance))
                _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(target, "ForcedSleep", comp.SleepDuration, true);

            _popupSystem.PopupPredicted(
                Loc.GetString("hit-head-popup-on-hit", ("attacker", attacker), ("target", target)),
                target,
                null,
                PopupType.MediumCaution
                );
        }

        _combatSystem.SetInCombatMode(attacker, previousCombatMode);
        comp.HitProccess = false;
        args.Handled = true;
    }

    private void OnGetMeleeDamage(Entity<HitHeadComponent> weapon, ref GetMeleeDamageEvent args)
    {
        var comp = weapon.Comp;

        if (!TryComp<MeleeWeaponComponent>(weapon, out var meleeWeapon) || !comp.HitProccess)
            return;

        var damage = meleeWeapon.Damage * comp.DamageMultiplier;
        args.Damage = damage;
        args.ResistanceBypass = true;
    }

    private bool CanBeHit(EntityUid target, EntityUid attacker, string protectionSlot = "head")
    {
        // Stupid to hit yourself
        if (target == attacker)
            return false;

        // If it don't have mob state it can't sleep
        if (!TryComp<MobStateComponent>(target, out var mobState))
            return false;

        // If target is dead or in crit we don't need to make it sleep
        if (_mobState.IsCritical(target, mobState) || _mobState.IsDead(target, mobState))
            return false;

        // If it can't get sleep 
        if (!HasComp<StatusEffectsComponent>(target))
            return false;

        // Attacker can't hit target if he can't attack
        if (!_actionBlocker.CanAttack(attacker))
            return false;

        // If target is sleeping already we don't need to stack the effect
        if (HasComp<SleepingComponent>(target))
            return false;

        // check if target has something on head
        if (!_inventorySystem.TryGetSlotEntity(target, protectionSlot, out var protectionEntity))
            return true;

        // check if headgear have protection
        if (HasComp<HitHeadProtectionComponent>(protectionEntity))
            return false;

        return true;
    }
}


[Serializable, NetSerializable]
public sealed partial class HitUnconsciousDoAfterEvent : SimpleDoAfterEvent
{
}
