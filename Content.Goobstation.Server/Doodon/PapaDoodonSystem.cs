// SPDX-FileCopyrightText: 2025 GoobBot
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Pointing;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Server logic for Papa Doodon (RatKing-style orders):
/// - Grants 4 separate order actions (Stay/Follow/Attack/Loose), each sets mode directly
/// - Uses SharedActionsSystem.SetToggled to drive iconOn for the active order (Rat King pattern)
/// - Point-to-attack target when in AttackTarget order
/// - Grants Establish Town Hall action until used (one-time)
///
/// IMPORTANT:
/// Do NOT call StartUseDelay in a periodic Update loop, or the icons will "flash" by constantly restarting cooldown.
/// </summary>
public sealed class PapaDoodonSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private const float CheckInterval = 1.0f;
    private float _accum;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PapaDoodonComponent, DoodonSetWarriorModeEvent>(OnSetMode);
        SubscribeLocalEvent<PapaDoodonComponent, AfterPointedAtEvent>(OnPointedAt);
        SubscribeLocalEvent<PapaDoodonComponent, DoodonEstablishTownHallEvent>(OnEstablishTownHall);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accum += frameTime;
        if (_accum < CheckInterval)
            return;

        _accum = 0f;

        var query = EntityQueryEnumerator<PapaDoodonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Only give actions to a controlled Papa
            if (!_mind.TryGetMind(uid, out _, out _))
                continue;

            EnsureAction(uid, ref comp.OrderStayActionEntity, comp.OrderStayAction);
            EnsureAction(uid, ref comp.OrderFollowActionEntity, comp.OrderFollowAction);
            EnsureAction(uid, ref comp.OrderAttackActionEntity, comp.OrderAttackAction);
            EnsureAction(uid, ref comp.OrderLooseActionEntity, comp.OrderLooseAction);

            // IMPORTANT: only SetToggled here (no StartUseDelay)
            UpdateOrderToggles(comp);

            // Establish hall is one-time.
            if (!comp.HallPlaced)
                EnsureAction(uid, ref comp.EstablishHallActionEntity, comp.EstablishHallAction);
            else
                RemoveAction(uid, ref comp.EstablishHallActionEntity);

            Dirty(uid, comp);
        }
    }

    private void EnsureAction(EntityUid owner, ref EntityUid? actionEnt, EntProtoId proto)
    {
        if (actionEnt == null || !Exists(actionEnt.Value))
            _actions.AddAction(owner, ref actionEnt, proto);
    }

    private void RemoveAction(EntityUid owner, ref EntityUid? actionEnt)
    {
        if (actionEnt is { } ent && Exists(ent))
            _actions.RemoveAction(owner, ent);

        actionEnt = null;
    }

    /// <summary>
    /// Drives iconOn for the active order.
    /// SAFE to call often.
    /// </summary>
    private void UpdateOrderToggles(PapaDoodonComponent comp)
    {
        _actions.SetToggled(comp.OrderStayActionEntity, comp.CurrentOrder == DoodonOrderType.Stay);
        _actions.SetToggled(comp.OrderFollowActionEntity, comp.CurrentOrder == DoodonOrderType.Follow);
        _actions.SetToggled(comp.OrderAttackActionEntity, comp.CurrentOrder == DoodonOrderType.AttackTarget);
        _actions.SetToggled(comp.OrderLooseActionEntity, comp.CurrentOrder == DoodonOrderType.Loose);
    }

    /// <summary>
    /// Optional UX: start use delay ONLY when the player clicks a command.
    /// </summary>
    private void StartOrderUseDelays(PapaDoodonComponent comp)
    {
        _actions.StartUseDelay(comp.OrderStayActionEntity);
        _actions.StartUseDelay(comp.OrderFollowActionEntity);
        _actions.StartUseDelay(comp.OrderAttackActionEntity);
        _actions.StartUseDelay(comp.OrderLooseActionEntity);
    }

    private void OnSetMode(EntityUid uid, PapaDoodonComponent comp, ref DoodonSetWarriorModeEvent args)
    {
        if (comp.CurrentOrder == args.Order)
            return;

        comp.CurrentOrder = args.Order;
        Dirty(uid, comp);

        ApplyOrderToWarriors(uid, comp);

        // Update iconOn highlight
        UpdateOrderToggles(comp);

        // OPTIONAL: match Rat King "button press" feel, but only on real click
        StartOrderUseDelays(comp);
    }

    private void ApplyOrderToWarriors(EntityUid papaUid, PapaDoodonComponent comp)
    {
        var papaFollowCoords = new EntityCoordinates(papaUid, Vector2.Zero);

        var query = EntityQueryEnumerator<DoodonWarriorComponent, HTNComponent>();
        while (query.MoveNext(out var warriorUid, out var warrior, out var htnComp))
        {
            if (warrior.Papa != papaUid)
                continue;

            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrders, comp.CurrentOrder);

            warrior.Orders = comp.CurrentOrder;
            Dirty(warriorUid, warrior);

            if (comp.CurrentOrder != DoodonOrderType.AttackTarget)
                _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrderedTarget, EntityUid.Invalid);

            if (comp.CurrentOrder == DoodonOrderType.Follow || comp.CurrentOrder == DoodonOrderType.AttackTarget)
                _npc.SetBlackboard(warriorUid, NPCBlackboard.FollowTarget, papaFollowCoords);
            else
                _npc.SetBlackboard(warriorUid, NPCBlackboard.FollowTarget, default(EntityCoordinates));

            if (htnComp.Plan != null)
                _htn.ShutdownPlan(htnComp);

            _htn.Replan(htnComp);
        }
    }

    private void OnPointedAt(EntityUid uid, PapaDoodonComponent comp, ref AfterPointedAtEvent args)
    {
        if (comp.CurrentOrder != DoodonOrderType.AttackTarget)
            return;

        if (args.Pointed == uid || Deleted(args.Pointed))
            return;

        var query = EntityQueryEnumerator<DoodonWarriorComponent, HTNComponent>();
        while (query.MoveNext(out var warriorUid, out var warrior, out var htnComp))
        {
            if (warrior.Papa != uid)
                continue;

            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrders, DoodonOrderType.AttackTarget);
            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrderedTarget, args.Pointed);

            warrior.Orders = DoodonOrderType.AttackTarget;
            Dirty(warriorUid, warrior);

            if (htnComp.Plan != null)
                _htn.ShutdownPlan(htnComp);

            _htn.Replan(htnComp);
        }

        // UI highlight should already be AttackTarget, but harmless
        UpdateOrderToggles(comp);
    }

    private void OnEstablishTownHall(EntityUid uid, PapaDoodonComponent comp, DoodonEstablishTownHallEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (comp.HallPlaced)
        {
            _popup.PopupEntity("You have already established a Town Hall.", uid, uid);
            return;
        }

        var coords = Transform(uid).Coordinates;
        Spawn(comp.TownHallPrototype, coords);

        comp.HallPlaced = true;

        RemoveAction(uid, ref comp.EstablishHallActionEntity);

        _popup.PopupEntity("You establish a Doodon Town Hall.", uid, uid);
        Dirty(uid, comp);
    }
}
