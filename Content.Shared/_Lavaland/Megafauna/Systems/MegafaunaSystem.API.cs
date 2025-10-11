using System.Numerics;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Actions;
using Robust.Shared.Map;
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
    public RequestPerformActionEvent GetPerformEvent(EntityUid boss, EntityUid action, string? entityKey, string? coordsKey)
    {
        var targetingComp = CompOrNull<MegafaunaAiTargetingComponent>(boss);
        var netAction = GetNetEntity(action);

        // I HATE DICTIONARIES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! and null logic.

        EntityUid? targetEnt = null;
        if (entityKey != null
            && (targetingComp?.Entities.TryGetValue(entityKey, out var dictEnt)?? false))
            targetEnt = dictEnt;

        EntityCoordinates? targetCoords = null;
        if (coordsKey != null
            && (targetingComp?.Coordinates.TryGetValue(coordsKey, out var dictCoords) ?? false))
            targetCoords = dictCoords;

        var netTarget = GetNetEntity(targetEnt);
        var netCoords = GetNetCoordinates(targetCoords);

        return new RequestPerformActionEvent(netAction, netTarget, netCoords);
    }

    /// <summary>
    /// Picks a new target based on bosses AggressiveComponent.
    /// </summary>
    public bool TryPickTargetAggressive(
        MegafaunaCalculationBaseArgs args,
        List<MegafaunaTargetCondition> conditions,
        string? entityKey,
        string? coordsKey,
        bool clearData = true)
    {
        if (!_aggressiveQuery.TryComp(args.Entity, out var aggressiveComp))
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
            args.Entity,
            entityKey,
            coordsKey,
            picked,
            Transform(picked.Value).Coordinates,
            clearData);
        return true;
    }

    public bool TryPickRandomPosition(
        MegafaunaCalculationBaseArgs args,
        string coordsKey,
        float radius)
    {
        // TODO add an option to not pick any obstructed coordinates

        var uid = args.Entity;
        var mapId = Transform(uid).MapID;

        var grid = _xform.GetGrid(uid);
        if (grid == null)
            return false;

        var randomVector = new Vector2(args.Random.NextFloat(-radius, radius), args.Random.NextFloat(-radius, radius));
        var position = _xform.GetWorldPosition(uid) + randomVector;
        var newMapCoords = new MapCoordinates(position, mapId);
        var coords = _xform.ToCoordinates(grid.Value, newMapCoords);

        return TrySetCoordinatesData(
            args.Entity,
            coordsKey,
            coords);
    }

    private void SetTargetingData(
        EntityUid bossEntity,
        string? entityKey,
        string? coordsKey,
        EntityUid? uid = null,
        EntityCoordinates? coords = null,
        bool clearData = true)
    {
        var targetComp = EnsureComp<MegafaunaAiTargetingComponent>(bossEntity);

        if (clearData)
        {
            if (entityKey != null && uid == null)
                targetComp.Entities.Remove(entityKey);

            if (coordsKey != null && coords == null)
                targetComp.Coordinates.Remove(coordsKey);
        }

        if (entityKey != null && uid != null)
            TrySetEntityData((bossEntity, targetComp), entityKey, uid.Value);

        if (coordsKey != null && coords != null)
            TrySetCoordinatesData((bossEntity, targetComp), coordsKey, coords.Value);
    }

    private bool TrySetEntityData(
        Entity<MegafaunaAiTargetingComponent?> ent,
        string entityKey,
        EntityUid entity)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (!ent.Comp.Entities.TryAdd(entityKey, entity))
            ent.Comp.Entities[entityKey] = entity;

        var marker = EnsureComp<MegafaunaTargetedComponent>(entity);
        marker.Targeted = ent.Owner;
        return true;
    }

    private bool TrySetCoordinatesData(
        Entity<MegafaunaAiTargetingComponent?> ent,
        string coordsKey,
        EntityCoordinates coords)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (!ent.Comp.Coordinates.TryAdd(coordsKey, coords))
            ent.Comp.Coordinates[coordsKey] = coords;

        var marker = EnsureComp<MegafaunaTargetedComponent>(coords.EntityId);
        marker.Targeted = ent.Owner;
        return true;
    }
}
