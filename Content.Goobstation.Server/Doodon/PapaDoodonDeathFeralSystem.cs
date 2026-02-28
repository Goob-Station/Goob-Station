using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Doodons;

public sealed class PapaDoodonDeathFeralSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PapaDoodonComponent, MobStateChangedEvent>(OnPapaMobStateChanged);
    }

    private void OnPapaMobStateChanged(EntityUid uid, PapaDoodonComponent papa, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!TryComp<DoodonComponent>(uid, out var doodon) ||
            doodon.TownHall is not { } hallUid ||
            Deleted(hallUid))
            return;

        EnsureComp<DoodonTownHallFeralComponent>(hallUid, out var feral);

        Dirty(hallUid, feral);

        if (!TryComp<DoodonTownHallComponent>(hallUid, out var hall))
            return;

        foreach (var d in hall.Doodons)
        {
            if (Deleted(d))
                continue;

            if (!TryComp<DoodonComponent>(d, out var dComp))
                continue;

            // Exclude moodons
            if (dComp.RequiredHousing == DoodonHousingType.Moodon)
                continue;

            // Clear all command-related blackboard state
            _npc.SetBlackboard(d, NPCBlackboard.FollowTarget, default(EntityCoordinates));
            _npc.SetBlackboard(d, NPCBlackboard.CurrentOrderedTarget, EntityUid.Invalid);

            if (TryComp<DoodonWarriorComponent>(d, out var warrior))
            {
                // Warriors go Loose
                _npc.SetBlackboard(d, NPCBlackboard.CurrentOrders, DoodonOrderType.Loose);

                warrior.Orders = DoodonOrderType.Loose;
                warrior.Papa = null;
                Dirty(d, warrior);
            }
            else
            {
                // Workers become hostile via faction system
                _factions.ClearFactions(d);
                _factions.AddFaction(d,"DoodonFeral");
            }

            // Force HTN refresh
            if (TryComp<HTNComponent>(d, out var htn))
            {
                if (htn.Plan != null)
                    _htn.ShutdownPlan(htn);

                _htn.Replan(htn);
            }
        }
    }
}
