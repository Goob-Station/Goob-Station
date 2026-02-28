using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Content.Goobstation.Shared.Doodons;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Content.Shared.CombatMode;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Utility helpers for wiring up a newly spawned warrior doodon.
/// </summary>
public sealed class DoodonWarriorSpawnHelperSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;

    /// <summary>
    /// Call this right after you Spawn() a WarriorDoodon from the hut.
    /// It finds the nearest Papa Doodon, assigns it, and sets blackboard keys so HTN works.
    /// </summary>
    public void InitializeWarriorFromHut(EntityUid warrior, EntityUid hut, float searchRange = 15f)
    {
        // Find nearest Papa within range of the hut.
        if (!TryFindNearestPapa(hut, searchRange, out var papaUid, out var papaComp))
            return;

        // 1) Assign ownership link (warrior -> papa)
        var warriorComp = EnsureComp<DoodonWarriorComponent>(warrior);
        warriorComp.Papa = papaUid;
        Dirty(warrior, warriorComp);

        // 2) Make FollowCompound actually move
        _npc.SetBlackboard(
            warrior,
            NPCBlackboard.FollowTarget,
            new EntityCoordinates(papaUid, Vector2.Zero));

        // 3) Let HasOrdersPrecondition read the order
        _npc.SetBlackboard(
            warrior,
            NPCBlackboard.CurrentOrders,
            papaComp.CurrentOrder);

        // (leave the rest of your existing init logic here)
    }

    private bool TryFindNearestPapa(EntityUid origin, float range, out EntityUid papaUid, out PapaDoodonComponent papaComp)
    {
        papaUid = default;
        papaComp = default!;

        var originCoords = Transform(origin).Coordinates;
        var originMapPos = Transform(origin).MapPosition;

        EntityUid? best = null;
        var bestDist2 = float.MaxValue;

        foreach (var ent in _lookup.GetEntitiesInRange(originCoords, range))
        {
            if (!TryComp(ent, out PapaDoodonComponent? papa))
                continue;

            var pos = Transform(ent).MapPosition;
            var d = pos.Position - originMapPos.Position;
            var dist2 = d.LengthSquared();

            if (dist2 >= bestDist2)
                continue;

            bestDist2 = dist2;
            best = ent;
            papaComp = papa;
        }

        if (best is not { } found)
            return false;

        papaUid = found;
        return true;
    }
}
