using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using FixedPoint2 = Content.Goobstation.Maths.FixedPoint.FixedPoint2;
using System.Linq;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Predicted half of the slasher's soul steal.
/// </summary>
public sealed class SlasherSoulStealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSoulStealComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSoulStealComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealEvent>(OnSoulSteal);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSummonMacheteEvent>(OnSummonMachete);
        SubscribeLocalEvent<SlasherSoulStealComponent, DidEquipHandEvent>(OnDidEquipHand);
        SubscribeLocalEvent<SlasherSoulStealComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnMapInit(Entity<SlasherSoulStealComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.ActionId);
        Dirty(ent);
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

        if (!TryComp<MindContainerComponent>(target, out var mindContainer) || !mindContainer.HasMind)
        {
            _popup.PopupClient(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        if (HasComp<SoullessComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        if (!HasComp<MobStateComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("slasher-soulsteal-fail-not-valid"), user, user);
            args.Handled = true;
            return;
        }

        if (!_mobState.IsCritical(target)
            && !_mobState.IsIncapacitated(target)
            && !_standing.IsDown(target)
            && !_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("slasher-soulsteal-fail-not-down"), user, user);
            args.Handled = true;
            return;
        }

        if (ent.Comp.RequireLimbLoss && !HasLimbLoss(target))
        {
            _popup.PopupClient(Loc.GetString("slasher-soulsteal-fail-no-limb-loss"), user, user);
            args.Handled = true;
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Soulstealdoafterduration,
            new SlasherSoulStealDoAfterEvent(), user, target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2f,
            RequireCanInteract = false
        });

        _popup.PopupClient(Loc.GetString("slasher-soulsteal-start", ("target", target)), user, user);

        // Popup for victim only.
        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start-victim", ("user", user)), target, target, PopupType.MediumCaution);

        args.Handled = true;
    }

    /// <summary>
    /// Whether the target is missing at least one arm, hand, leg or foot.
    /// </summary>
    public bool HasLimbLoss(EntityUid target)
    {
        foreach (var (partId, part) in _body.GetBodyChildren(target))
            foreach (var (slotId, slot) in part.Children)
            {
                if (slot.Type is not (BodyPartType.Arm or BodyPartType.Hand or BodyPartType.Leg or BodyPartType.Foot))
                    continue;

                if (!_container.TryGetContainer(partId, SharedBodySystem.GetPartSlotContainerId(slotId), out var container)
                    || container.ContainedEntities.Count == 0)
                    return true;
            }

        return false;
    }

    public void ApplyArmorBonus(EntityUid user, float percent, SlasherSoulStealComponent comp)
    {
        if (percent <= 0f)
            return;

        comp.ArmorReduction = MathF.Min(comp.ArmorReduction + percent, comp.ArmorCap);
        Dirty(user, comp);
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

    public EntityUid? GetMachete(EntityUid user)
    {
        if (TryComp<SlasherSummonMacheteComponent>(user, out var summon)
            && summon.MacheteUid != null
            && Exists(summon.MacheteUid.Value))
            return summon.MacheteUid.Value;

        if (!TryComp<HandsComponent>(user, out var hands))
            return null;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
            if (HasComp<SlasherMassacreMacheteComponent>(held))
                return held;

        return null;
    }

    // Apply brute bonus to machete
    public void ApplyMacheteBonus(EntityUid user, float bruteBonus, SlasherSoulStealComponent comp)
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
        Dirty(user, comp);
    }

    /// <summary>
    /// Slasher - Handles the machete bonus damage from stealing souls
    /// </summary>
    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f)
            return;

        var add = new DamageSpecifier();

        add.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        args.Damage += add;
    }

    /// <summary>
    /// Slasher - Handles summoning the Machete to the slasher (self)
    /// </summary>
    private void OnSummonMachete(Entity<SlasherSoulStealComponent> ent, ref SlasherSummonMacheteEvent args)
    {
        var machete = GetMachete(ent.Owner);

        if (machete == null)
            return;

        ent.Comp.LastMachete = machete.Value;

        if (ent.Comp.TotalAppliedBruteBonus > 0f)
        {
            var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete.Value);
            bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
            Dirty(machete.Value, bonusComp);
        }

        Dirty(ent);
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
