using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Pointing;
using Robust.Shared.Map;
using System;
using System.Numerics;

namespace Content.Goobstation.Server.Doodons;

public sealed class PapaDoodonSystem : SharedPapaDoodonSystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PapaDoodonComponent, PapaDoodonCommandActionEvent>(OnCommand);
        SubscribeLocalEvent<PapaDoodonComponent, AfterPointedAtEvent>(OnPointedAt);
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

            // 2) Keep the component field in sync (so VV shows the real order)
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

            Logger.Info($"Ordering warrior {warriorUid} to AttackTarget; target={args.Pointed}");

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
}

