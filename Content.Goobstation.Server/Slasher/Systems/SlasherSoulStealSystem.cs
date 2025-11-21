using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Goobstation.Shared.Slasher.Objectives;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Shared.Timing;
using FixedPoint2 = Content.Goobstation.Maths.FixedPoint.FixedPoint2;
using System.Linq;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Soul steal system for the slasher. Gives bonuses for stealing souls from incapacitated or dead targets.
/// </summary>
public sealed class SlasherSoulStealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DevilContractSystem _devilContractSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSoulStealComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSoulStealComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealEvent>(OnSoulSteal);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealDoAfterEvent>(OnSoulStealDoAfterComplete);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, ThrowDoHitEvent>(OnThrowHit);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSummonMacheteEvent>(OnSummonMachete);
        SubscribeLocalEvent<SlasherSoulStealComponent, DidEquipHandEvent>(OnDidEquipHand);
        SubscribeLocalEvent<SlasherSoulStealComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnMapInit(Entity<SlasherSoulStealComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherSoulStealComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }

    /// <summary>
    /// Handles the soul steal event
    /// </summary>
    /// <param name="ent">Owner of the SlasherSoulStealComponent</param>
    /// <param name="args">SlasherSoulStealEvent</param>
    private void OnSoulSteal(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealEvent args)
    {
        if (args.Handled || !args.Target.Valid)
            return;

        var user = ent.Owner;
        var target = args.Target;

        // Check rather our victim has a mind
        if (!_mindSystem.TryGetMind(target, out _, out MindComponent? _))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        // Can't steal soul from the same person multiple times
        if (HasComp<SoullessComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        // Must be a valid mob
        if (!HasComp<MobStateComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-not-valid"), user, user);
            args.Handled = true;
            return;
        }

        // check if target is downed, incapacitated, or dead
        if (!CanStartSoulSteal(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-not-down"), user, user);
            args.Handled = true;
            return;
        }

        // DoAfter, starting the do-after to the next tick to avoid modifying ActiveDoAfterComponent when active.
        Timer.Spawn(_timing.TickPeriod, () =>
        {
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Soulstealdoafterduration,
                new SlasherSoulStealDoAfterEvent(), user, target: target)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                DistanceThreshold = 2f,
                RequireCanInteract = false
            });
        });

        // Popup for user
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start", ("target", target)), user, user);
        // Popup for victim only
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start-victim", ("user", user)), target, target, PopupType.MediumCaution);
        args.Handled = true;
    }

    // Checks to ensure our target is valid (alive & not downed, incapacitated, or dead)
    private bool CanStartSoulSteal(EntityUid target)
    {
        return _mobState.IsCritical(target)
               || _mobState.IsIncapacitated(target)
               || _standing.IsDown(target)
               || _mobState.IsDead(target);
    }

    /// <summary>
    /// Slasher - Handles the soul steal do-after
    /// </summary>
    /// <param name="ent">SlasherSoulStealComponent</param>
    /// <param name="ev">SlasherSoulStealDoAfterEvent</param>
    private void OnSoulStealDoAfterComplete(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Args.Target == null)
            return;

        var user = ent.Owner;
        var target = ev.Args.Target.Value;
        var comp = ent.Comp;

        _audio.PlayPvs(ent.Comp.SoulStealSound, target);

        var alive = _mobState.IsAlive(target);

        var bruteBonus = alive ? comp.AliveBruteBonusPerSoul : comp.DeadBruteBonusPerSoul;
        var armorBonus = alive ? comp.AliveArmorPercentPerSoul : comp.DeadArmorPercentPerSoul;

        if (alive)
            comp.AliveSouls++;
        else
            comp.DeadSouls++;

        // Update absorb souls objective progress
        if (_mindSystem.TryGetMind(user, out var mindId, out var mind))
        {
            foreach (var objUid in mind.Objectives)
            {
                if (!TryComp<SlasherAbsorbSoulsConditionComponent>(objUid, out var absorbObj))
                    continue;

                absorbObj.Absorbed += 1;
                Dirty(objUid, absorbObj);
                break;
            }
        }

        // Apply devil clause downside
        _devilContractSystem.AddRandomNegativeClause(target);

        // Used to prevent stealing from the same person multiple times
        EnsureComp<SoullessComponent>(target);

        //TryFlavorTwistLimbs(user, target); // TODO Originally intended to take off their limbs and replace them with limbs from random species but I couldn't get it working properly
        ApplyArmorBonus(user, armorBonus, comp);
        ApplyMacheteBonus(user, bruteBonus, comp);

        // Popup for user
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success", ("target", target)), user, user, PopupType.LargeCaution);
        // Popup for victim only
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success-victim", ("user", user)), target, target, PopupType.LargeCaution);
        Dirty(user, comp);
    }

    private void ApplyArmorBonus(EntityUid user, float percent, SlasherSoulStealComponent comp)
    {
        if (percent <= 0f)
            return;

        comp.ArmorReduction = MathF.Min(comp.ArmorReduction + percent, comp.ArmorCap);
    }

    private void OnDamageModify(Entity<SlasherSoulStealComponent> ent, ref DamageModifyEvent args)
    {
        var reduction = ent.Comp.ArmorReduction;
        if (reduction <= 0f || args.Damage.Empty)
            return;

        var pairs = args.Damage.DamageDict.ToArray();
        var factor = 1f - reduction;
        foreach (var kv in pairs)
        {
            var type = kv.Key;
            var val = kv.Value;
            if (val <= FixedPoint2.Zero)
                continue; // don't scale healing
            args.Damage.DamageDict[type] = val * factor;
        }
    }

    // Check machete to increase damage bonus
    private EntityUid? GetMachete(EntityUid user)
    {
        if (TryComp<SlasherSummonMacheteComponent>(user, out var summon)
            && summon.MacheteUid != null
            && Exists(summon.MacheteUid.Value))
            return summon.MacheteUid.Value;

        if (!TryComp<HandsComponent>(user, out var hands))
            return null;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
        {
            if (HasComp<SlasherMassacreMacheteComponent>(held))
                return held;
        }

        return null;
    }

    // Apply brute bonus to machete
    private void ApplyMacheteBonus(EntityUid user, float bruteBonus, SlasherSoulStealComponent comp)
    {
        if (bruteBonus <= 0f)
            return;

        var machete = GetMachete(user);
        if (machete == null)
            return;

        var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete.Value);
        bonusComp.SlashBonus += bruteBonus;
        comp.TotalAppliedBruteBonus += bruteBonus;
        comp.LastMachete = machete.Value;
        Dirty(machete.Value, bonusComp);
    }

    /// <summary>
    /// Slasher - Handles the machete bonus damage from stealing souls
    /// </summary>
    /// <param name="ent">SlasherSoulStealMacheteBonusComponent</param>
    /// <param name="args">GetMeleeDamageEvent</param>
    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f)
            return;

        var add = new DamageSpecifier();

        add.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        args.Damage += add;
    }

    private void OnThrowHit(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f || TerminatingOrDeleted(args.Target))
            return;

        var dmgAdj = new DamageSpecifier();

        dmgAdj.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        _damageable.TryChangeDamage(args.Target, dmgAdj, true, origin: args.Component.Thrower);
    }

    /// <summary>
    /// Slasher - Handles summoning the Machete to the slasher (self)
    /// </summary>
    /// <param name="ent">SlasherSoulStealComponent</param>
    /// <param name="args">SlasherSummonMacheteEvent</param>
    private void OnSummonMachete(Entity<SlasherSoulStealComponent> ent, ref SlasherSummonMacheteEvent args)
    {
        var machete = GetMachete(ent.Owner);

        if (machete != null)
        {
            ent.Comp.LastMachete = machete.Value;

            if (ent.Comp.TotalAppliedBruteBonus > 0f)
            {
                var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete.Value);
                bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
                Dirty(machete.Value, bonusComp);
            }

            Dirty(ent);
        }
    }

    private void OnDidEquipHand(Entity<SlasherSoulStealComponent> ent, ref DidEquipHandEvent args)
    {
        if (!HasComp<SlasherMassacreMacheteComponent>(args.Equipped))
            return;

        ent.Comp.LastMachete = args.Equipped;

        if (ent.Comp.TotalAppliedBruteBonus > 0f)
        {
            var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(args.Equipped);
            bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
            Dirty(args.Equipped, bonusComp);
        }

        Dirty(ent);
    }
}
