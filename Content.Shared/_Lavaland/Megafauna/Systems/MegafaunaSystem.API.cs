using System.Numerics;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

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
        var netTarget = GetNetEntity(targetingComp?.TargetEnt);
        var netCoords = GetNetCoordinates(targetingComp?.TargetCoords);

        return new RequestPerformActionEvent(netAction, netTarget, netCoords);
    }

    /// <summary>
    /// Picks a new target based on bosses AggressiveComponent.
    /// </summary>
    public bool TryPickTargetAggressive(
        MegafaunaCalculationBaseArgs args,
        List<MegafaunaTargetCondition> conditions,
        bool setPosition = false)
    {
        if (!_aggressiveQuery.TryComp(args.Entity, out var aggressiveComp))
        {
            DebugTools.Assert($"Megafauna AI doesn't have {nameof(AggressiveComponent)}, but tried to pick a target using it's data!");
            return false;
        }

        // Check all conditions on all possible targets
        var results = new Dictionary<EntityUid, float>();
        foreach (var target in aggressiveComp.Aggressors)
        {
            var weight = 0f;
            foreach (var condition in conditions)
            {
                weight += condition.Evaluate(args, target);
            }

            results.Add(target, weight);
        }

        var maxWeight = float.MinValue;
        EntityUid? picked = null;
        foreach (var (target, fails) in results)
        {
            if (maxWeight < fails)
            {
                maxWeight = fails;
                picked = target;
            }
        }

        if (picked == null)
        {
            DebugTools.Assert($"Megafauna AI failed to pick a target from {nameof(AggressiveComponent)}, so it doesn't have any targets to pick from.");
            return false;
        }

        var comp = EnsureComp<MegafaunaAiTargetingComponent>(args.Entity);
        comp.TargetEnt = picked.Value;
        comp.TargetCoords = null;

        if (setPosition)
            comp.TargetCoords = Transform(picked.Value).Coordinates;

        return true;
    }

    public void PickRandomPosition(MegafaunaCalculationBaseArgs args, float radius)
    {
        // TODO add an option to not pick any obstructed coordinates

        var uid = args.Entity;
        var mapId = Transform(uid).MapID;

        var randomVector = new Vector2(args.Random.NextFloat(-radius, radius), args.Random.NextFloat(-radius, radius));
        var position = _xform.GetWorldPosition(uid) + randomVector;
        var newMapCoords = new MapCoordinates(position, mapId);
        var coords = _xform.ToCoordinates(newMapCoords);

        var comp = EnsureComp<MegafaunaAiTargetingComponent>(args.Entity);
        comp.TargetEnt = null;
        comp.TargetCoords = coords;
    }
}
