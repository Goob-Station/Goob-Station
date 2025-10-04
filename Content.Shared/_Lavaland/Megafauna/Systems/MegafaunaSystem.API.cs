using System.Linq;
using System.Numerics;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    public void StartupMegafauna(Entity<MegafaunaAiComponent> ent)
    {
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
        ent.Comp.Active = true;
    }

    public void ShutdownMegafauna(Entity<MegafaunaAiComponent> ent)
    {
        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());
        ent.Comp.Active = false;
    }

    public void KillMegafauna(Entity<MegafaunaAiComponent> ent)
    {
        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());
        RaiseLocalEvent(ent, new MegafaunaKilledEvent());
        ent.Comp.Active = false;
    }

    /// <summary>
    /// Helper method that constructs new <see cref="RequestPerformActionEvent"/> for megafauna AI to use an action.
    /// </summary>
    public RequestPerformActionEvent GetPerformEvent(EntityUid boss, EntityUid action)
    {
        var targetingComp = CompOrNull<MegafaunaAiTargetingComponent>(boss);
        var netAction = GetNetEntity(action);
        var netTarget = GetNetEntity(targetingComp?.TargetEntity);
        var netCoords = GetNetCoordinates(targetingComp?.TargetCoordinates);

        return new RequestPerformActionEvent(netAction, netTarget, netCoords);
    }

    /// <summary>
    /// Picks a new target based on bosses AggressiveComponent.
    /// </summary>
    public bool TryPickTargetAggressive(
        MegafaunaCalculationBaseArgs args,
        List<MegafaunaTargetCondition> conditions,
        bool setTarget = true,
        bool setCoordinates = true,
        bool clearData = true)
    {
        if (!_aggressiveQuery.TryComp(args.BossEntity, out var aggressiveComp))
            return false;

        var results = new Dictionary<EntityUid, int>();
        foreach (var target in aggressiveComp.Aggressors)
        {
            var fails = 0;
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate(args, target))
                    fails++;
            }

            results.Add(target, fails);
        }

        // I bet that this code sucks. If someone actually knows how to code please fix.
        var leastFails = int.MaxValue;
        foreach (var (_, fails) in results)
        {
            if (leastFails > fails)
                leastFails = fails;
        }

        EntityUid? picked = null;
        foreach (var (target, fails) in results)
        {
            if (fails == leastFails)
                picked = target;
        }

        if (picked == null)
            return false;

        SetTargetingData(
            args.BossEntity,
            setTarget ? picked : null,
            setCoordinates ? Transform(picked.Value).Coordinates : null,
            clearData);
        return true;
    }

    public bool TryPickRandomPosition(MegafaunaCalculationBaseArgs args, float radius = 4f, bool alignTile = false)
    {
        MapCoordinates? newMapCoords = null;
        var uid = args.BossEntity;
        var mapId = Transform(uid).MapID;

        var grid = _xform.GetGrid(uid);
        if (grid == null)
            return false;

        for (var i = 0; i < 20; i++)
        {
            var randomVector = new Vector2(args.Random.NextFloat(-4, 4), args.Random.NextFloat(-4, 4));
            var position = _xform.GetWorldPosition(uid) + randomVector;
            var checkBox = Box2.CenteredAround(position, new Vector2i(2, 2));

            var ents = _map.GetAnchoredEntities(grid.Value, Comp<MapGridComponent>(grid.Value), checkBox);
            if (ents.Any())
                continue;

            newMapCoords = new MapCoordinates(position, mapId);
            break;
        }

        if (newMapCoords == null)
            return false;

        var coords = _xform.ToCoordinates(newMapCoords.Value);
        coords = _xform.WithEntityId(coords, grid.Value);
        SetTargetingData(
            args.BossEntity,
            null,
            coords);
        return true;
    }

    private void SetTargetingData(
        EntityUid bossEntity,
        EntityUid? uid = null,
        EntityCoordinates? coords = null,
        bool clearData = true)
    {
        var targetComp = EnsureComp<MegafaunaAiTargetingComponent>(bossEntity);

        if (uid != null || clearData)
            targetComp.TargetEntity = uid;
        if (coords != null || clearData)
            targetComp.TargetCoordinates = coords;
    }
}
