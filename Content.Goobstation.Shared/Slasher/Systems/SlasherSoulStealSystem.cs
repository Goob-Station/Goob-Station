using System.Linq;
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

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles slashers soulsteal / ascension.
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

        args.Handled = true;

        var user = ent.Owner;
        var target = args.Target;
        if (GetFailReason(ent, target) is { } reason)
        {
            _popup.PopupClient(Loc.GetString(reason), user, user);
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            user,
            ent.Comp.Soulstealdoafterduration,
            new SlasherSoulStealDoAfterEvent(),
            user,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2f,
            RequireCanInteract = false,
        });

        _popup.PopupClient(Loc.GetString("slasher-soulsteal-start", ("target", target)), user, user);

        if (_net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start-victim", ("user", user)),
                target,
                target,
                PopupType.MediumCaution);
        }
    }

    private LocId? GetFailReason(Entity<SlasherSoulStealComponent> ent, EntityUid target)
    {
        if (!TryComp<MindContainerComponent>(target, out var mindContainer) || !mindContainer.HasMind)
            return "slasher-soulsteal-fail-no-mind";

        // Can't steal a soul from the same person twice.
        if (HasComp<SoullessComponent>(target))
            return "slasher-soulsteal-fail-no-mind";

        if (!HasComp<MobStateComponent>(target))
            return "slasher-soulsteal-fail-not-valid";

        if (!_mobState.IsCritical(target)
            && !_mobState.IsIncapacitated(target)
            && !_standing.IsDown(target)
            && !_mobState.IsDead(target))
            return "slasher-soulsteal-fail-not-down";

        if (ent.Comp.RequireLimbLoss && !HasLimbLoss(target))
            return "slasher-soulsteal-fail-no-limb-loss";

        return null;
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

                var containerId = SharedBodySystem.GetPartSlotContainerId(slotId);
                if (!_container.TryGetContainer(partId, containerId, out var container)
                    || container.ContainedEntities.Count == 0)
                    return true;
            }

        return false;
    }

    public void ApplyArmorBonus(Entity<SlasherSoulStealComponent> ent, float percent)
    {
        if (percent <= 0f)
            return;

        ent.Comp.ArmorReduction = MathF.Min(ent.Comp.ArmorReduction + percent, ent.Comp.ArmorCap);
        Dirty(ent);
    }

    public void ApplyMacheteBonus(Entity<SlasherSoulStealComponent> ent, float bruteBonus)
    {
        if (bruteBonus <= 0f || GetMachete(ent.Owner) is not { } machete)
            return;

        var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete);
        bonusComp.SlashBonus += bruteBonus;
        Dirty(machete, bonusComp);

        ent.Comp.TotalAppliedBruteBonus += bruteBonus;
        ent.Comp.LastMachete = machete;
        Dirty(ent);
    }

    private void OnDamageModify(Entity<SlasherSoulStealComponent> ent, ref DamageModifyEvent args)
    {
        var reduction = ent.Comp.ArmorReduction;
        if (reduction <= 0f || args.Damage.Empty)
            return;

        var factor = 1f - reduction;
        foreach (var (type, value) in args.Damage.DamageDict.ToArray())
        {
            // Don't scale healing.
            if (value <= FixedPoint2.Zero)
                continue;

            args.Damage.DamageDict[type] = value * factor;
        }
    }

    private EntityUid? GetMachete(EntityUid user)
    {
        if (TryComp<SlasherSummonMacheteComponent>(user, out var summon)
            && summon.MacheteUid is { } summoned
            && Exists(summoned))
            return summoned;

        if (!TryComp<HandsComponent>(user, out var hands))
            return null;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
            if (HasComp<SlasherMassacreMacheteComponent>(held))
                return held;

        return null;
    }

    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f)
            return;

        var add = new DamageSpecifier();
        add.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        args.Damage += add;
    }

    private void OnSummonMachete(Entity<SlasherSoulStealComponent> ent, ref SlasherSummonMacheteEvent args)
    {
        if (GetMachete(ent.Owner) is { } machete)
            CarryBonusTo(ent, machete);
    }

    private void OnDidEquipHand(Entity<SlasherSoulStealComponent> ent, ref DidEquipHandEvent args)
    {
        if (HasComp<SlasherMassacreMacheteComponent>(args.Equipped))
            CarryBonusTo(ent, args.Equipped);
    }

    private void CarryBonusTo(Entity<SlasherSoulStealComponent> ent, EntityUid machete)
    {
        ent.Comp.LastMachete = machete;
        Dirty(ent);

        if (ent.Comp.TotalAppliedBruteBonus <= 0f)
            return;

        var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete);
        bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
        Dirty(machete, bonusComp);
    }
}
