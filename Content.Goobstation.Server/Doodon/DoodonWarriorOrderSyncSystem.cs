using System.Collections.Generic;
using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;

namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonWarriorOrderSyncSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;

    // last order we applied -> blackboard, per entity
    private readonly Dictionary<EntityUid, DoodonOrderType> _lastApplied = new();

    private float _accum;

    public override void Update(float frameTime)
    {
        _accum += frameTime;
        if (_accum < 0.25f) // sync 4 times/sec
            return;

        _accum = 0f;

        var query = EntityQueryEnumerator<DoodonWarriorComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var warrior, out var htn))
        {
            if (_lastApplied.TryGetValue(uid, out var last) && last == warrior.Orders)
                continue;

            // Apply component Orders -> blackboard (what HasOrdersPrecondition reads)
            _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrders, warrior.Orders);

            // Replan so it takes effect immediately
            if (htn.Plan != null)
                _htn.ShutdownPlan(htn);

            _htn.Replan(htn);

            _lastApplied[uid] = warrior.Orders;
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _lastApplied.Clear();
    }
}

