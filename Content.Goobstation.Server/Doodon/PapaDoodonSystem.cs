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
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Server logic for Papa Doodon:
/// - Cycles warrior orders with command action
/// - Point-to-attack target when in AttackTarget order
/// - Grants two actions when controlled:
///   - Establish Town Hall (one-time)
///   - Toggle Town Hall influence radius display (toggles nearest hall)
/// </summary>
public sealed class PapaDoodonSystem : SharedPapaDoodonSystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float CheckInterval = 1.0f;
    private float _accum;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PapaDoodonComponent, PapaDoodonCommandActionEvent>(OnCommand);
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

        // Give actions once Papa is actually controlled (has a mind).
        var query = EntityQueryEnumerator<PapaDoodonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Only give actions to a controlled Papa (prevents wild Papas from getting hotbar actions)
            if (!_mind.TryGetMind(uid, out _, out _))
                continue;

            // Establish hall is one-time.
            if (!comp.HallPlaced)
            {
                if (comp.EstablishHallActionEntity == null || !Exists(comp.EstablishHallActionEntity.Value))
                    _actions.AddAction(uid, ref comp.EstablishHallActionEntity, comp.EstablishHallAction);
            }
            else
            {
                if (comp.EstablishHallActionEntity != null && Exists(comp.EstablishHallActionEntity.Value))
                    _actions.RemoveAction(comp.EstablishHallActionEntity.Value);

                comp.EstablishHallActionEntity = null;
            }

            Dirty(uid, comp);
        }
    }

    private static DoodonOrderType Next(DoodonOrderType cur)
    {
        return cur switch
        {
            DoodonOrderType.Stay => DoodonOrderType.Follow,
            DoodonOrderType.Follow => DoodonOrderType.AttackTarget,
            DoodonOrderType.AttackTarget => DoodonOrderType.Loose,
            _ => DoodonOrderType.Stay
        };
    }

    private void OnCommand(EntityUid uid, PapaDoodonComponent comp, PapaDoodonCommandActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        comp.CurrentOrder = Next(comp.CurrentOrder);
        Dirty(uid, comp);
        UpdateCommandAction(uid, comp);

        var papaFollowCoords = new EntityCoordinates(uid, Vector2.Zero);

        var query = EntityQueryEnumerator<DoodonWarriorComponent, HTNComponent>();
        while (query.MoveNext(out var warriorUid, out var warrior, out var htnComp))
        {
            if (warrior.Papa != uid)
                continue;

            // 1) Set the HTN order (what HasOrdersPrecondition reads)
            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrders, comp.CurrentOrder);

            // 2) Keep component field in sync (so VV shows the real order)
            warrior.Orders = comp.CurrentOrder;
            Dirty(warriorUid, warrior);

            // 3) Clear ordered target unless we’re in AttackTarget mode
            if (comp.CurrentOrder != DoodonOrderType.AttackTarget)
                _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrderedTarget, EntityUid.Invalid);

            // 4) FollowTarget policy:
            // Follow + AttackTarget: keep a follow anchor to papa
            // Stay + Loose: clear it so Loose won’t “shadow-follow”
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

            // Force them into attack mode and set ordered target
            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrders, DoodonOrderType.AttackTarget);
            _npc.SetBlackboard(warriorUid, NPCBlackboard.CurrentOrderedTarget, args.Pointed);

            // Keep component field in sync for VV/debugging
            warrior.Orders = DoodonOrderType.AttackTarget;
            Dirty(warriorUid, warrior);

            if (htnComp.Plan != null)
                _htn.ShutdownPlan(htnComp);

            _htn.Replan(htnComp);
        }
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

        // Spawn at Papa’s feet
        var coords = Transform(uid).Coordinates;
        Spawn(comp.TownHallPrototype, coords);

        comp.HallPlaced = true;

        // Remove the one-time action
        if (comp.EstablishHallActionEntity != null && Exists(comp.EstablishHallActionEntity.Value))
            _actions.RemoveAction(comp.EstablishHallActionEntity.Value);

        comp.EstablishHallActionEntity = null;

        _popup.PopupEntity("You establish a Doodon Town Hall.", uid, uid);
        Dirty(uid, comp);
    }
}
