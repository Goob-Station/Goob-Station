using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Actions;

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
        var netCoords = GetNetCoordinates(targetingComp?.TargetCoordinate);

        return new RequestPerformActionEvent(netAction, netTarget, netCoords);
    }

    /// <summary>
    /// Picks a new target based on bosses AggressiveComponent.
    /// </summary>
    public bool TryPickTargetAggressive(
        MegafaunaCalculationBaseArgs args,
        List<MegafaunaTargetCondition> conditions,
        bool setTarget,
        bool setCoordinates,
        bool clearAll)
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

        var targetComp = EnsureComp<MegafaunaAiTargetingComponent>(args.BossEntity);
        targetComp.TargetEntity = picked.Value;
        targetComp.TargetCoordinate = Transform(picked.Value).Coordinates;

        return true;
    }
}
