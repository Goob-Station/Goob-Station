using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Throwing;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands;
using Content.Shared.Stunnable;
using Content.Shared.Mobs;
using Content.Shared.Bed.Sleep;
using Content.Shared.Standing;
using Content.Shared.Mind;
using FixedPoint2 = Content.Goobstation.Maths.FixedPoint.FixedPoint2;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Soul steal system for the slasher. Gives bonuses for stealing souls from downed or dead targets.
/// </summary>
public sealed class SlasherSoulStealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobThresholdSystem _thresholds = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DevilContractSystem _devilContractSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

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
    }

    private void OnMapInit(Entity<SlasherSoulStealComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherSoulStealComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }

    private void OnSoulSteal(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealEvent args)
    {
        if (args.Handled || !args.Target.Valid)
            return;

        var user = ent.Owner;
        var target = args.Target;

        // Require the victim to have a mind
        if (!_mindSystem.TryGetMind(target, out _, out MindComponent? _))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        // Can't steal soul from the same person multiple times
        if (HasComp<SoullessComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-soulless"), user, user);
            args.Handled = true;
            return;
        }

        if (!CanStartSoulSteal(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-not-down"), user, user);
            args.Handled = true;
            return;
        }


        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Soulstealdoafterduration, new SlasherSoulStealDoAfterEvent(), user, target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2f,
            RequireCanInteract = false
        });

        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start", ("target", target)), user, user);
        args.Handled = true;
    }

    // Basic checks to ensure they're a valid target
    private bool CanStartSoulSteal(EntityUid target)
    {
        if (!HasComp<MobStateComponent>(target))
            return false;

        if (_mobState.IsDead(target))
            return true;

        if (_mobState.IsCritical(target) || _mobState.IsIncapacitated(target) || _standing.IsDown(target))
            return true;

        return HasComp<KnockedDownComponent>(target) || HasComp<StunnedComponent>(target) || HasComp<SleepingComponent>(target);
    }

    private void OnSoulStealDoAfterComplete(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Args.Target == null)
            return;


        CompleteSoulSteal(ent.Owner, ev.Args.Target.Value, ent.Comp);
    }

    private void CompleteSoulSteal(EntityUid user, EntityUid target, SlasherSoulStealComponent comp)
    {
        var alive = _mobState.IsAlive(target);
        var dead = _mobState.IsDead(target);

        var bruteBonus = alive ? comp.AliveBruteBonusPerSoul : comp.DeadBruteBonusPerSoul;
        var healthBonus = alive ? comp.AliveHealthBonusPerSoul : comp.DeadHealthBonusPerSoul;

        if (alive)
            comp.AliveSouls++;
        else
            comp.DeadSouls++;

        // Apply devil clause downside
        _devilContractSystem.AddRandomNegativeClause(target);
        EnsureComp<SoullessComponent>(target);

        //TryFlavorTwistLimbs(user, target); // Originally intended to take off their limbs and replace them with limbs from random species but I couldn't get it working properly
        ApplyHealthBonus(user, healthBonus, comp);
        ApplyMacheteBonus(user, bruteBonus, comp);

        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success", ("target", target)), user, user, PopupType.Large);
        Dirty(user, comp);
    }

    private void ApplyHealthBonus(EntityUid user, int bonus, SlasherSoulStealComponent comp)
    {
        if (bonus <= 0 || !TryComp(user, out MobThresholdsComponent? thresholds))
            return;

        // Increase both the Critical and Dead thresholds.
        if (_thresholds.TryGetThresholdForState(user, MobState.Critical, out var critThreshold, thresholds)
            && critThreshold != null)
            _thresholds.SetMobStateThreshold(user, critThreshold.Value + bonus, MobState.Critical, thresholds);

        if (_thresholds.TryGetThresholdForState(user, MobState.Dead, out var deadThreshold, thresholds)
            && deadThreshold != null)
            _thresholds.SetMobStateThreshold(user, deadThreshold.Value + bonus, MobState.Dead, thresholds);


        comp.TotalAppliedHealthBonus += bonus;
    }

    // Check machete to increase damage bonus
    private EntityUid? GetMachete(EntityUid user)
    {
        if (TryComp<SlasherSummonMacheteComponent>(user, out var summon) && summon.MacheteUid != null && Exists(summon.MacheteUid.Value))
            return summon.MacheteUid.Value;

        if (!TryComp<HandsComponent>(user, out var hands))
            return null;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
            if (HasComp<SlasherMassacreMacheteComponent>(held))
                return held;

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

    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f)
            return;
        var add = new DamageSpecifier();
        add.DamageDict.Add("Slash", (FixedPoint2) ent.Comp.SlashBonus);
        args.Damage += add;
    }

    private void OnThrowHit(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f || TerminatingOrDeleted(args.Target))
            return;
        var dmgAdj = new DamageSpecifier();
        dmgAdj.DamageDict.Add("Slash", (FixedPoint2) ent.Comp.SlashBonus);
        _damageable.TryChangeDamage(args.Target, dmgAdj, true, origin: args.Component.Thrower);
    }

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

    // Supposed to give random limbs to victims but I couldn't get it working
    /*
    private void TryFlavorTwistLimbs(EntityUid user, EntityUid target)
    {
        // Amputate the victim
        var ev = new SlasherSoulStealAmputateEvent(target, user);
        RaiseLocalEvent(ev);

        // Give the victim new arms / legs

        // Give the victim new hands / feet
    }

    private void OnSoulStealAmputate(SlasherSoulStealAmputateEvent ev)
    {
        var target = ev.Target;

        // Collect limbs to amputate. We spare head and chest.
        var parts = _body.GetBodyChildren(target);
        var distal = new List<EntityUid>(); // hands/feet first
        var proximal = new List<EntityUid>(); // arms/legs second

        foreach (var part in parts)
        {
            switch (part.Component.PartType)
            {
                case BodyPartType.Hand:
                case BodyPartType.Foot:
                    distal.Add(part.Id);
                    break;
                case BodyPartType.Arm:
                case BodyPartType.Leg:
                    proximal.Add(part.Id);
                    break;
            }
        }

        void TryAmputate(EntityUid limb)
        {
            if (!TryComp<WoundableComponent>(limb, out var woundable) || !woundable.ParentWoundable.HasValue)
                return;
            var parent = woundable.ParentWoundable.Value;
            _wounds.AmputateWoundableSafely(parent, limb, woundable);
        }

        foreach (var limb in distal)
            TryAmputate(limb);
        foreach (var limb in proximal)
            TryAmputate(limb);
    }
    */
}
