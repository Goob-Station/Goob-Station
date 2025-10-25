using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared.Random.Helpers;
using Robust.Shared.Utility;

// ReSharper disable once CheckNamespace
namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Uses AggressiveComponent to pick a new target to attack.
/// Stores the result in a <see cref="MegafaunaAiTargetingComponent"/>
/// as EntityUid and EntityCoordinates of the entity.
/// </summary>
public sealed partial class AggressivePickTargetSelector : MegafaunaSelector
{
    /// <summary>
    /// Checks that will run on all possible target entities. Then selector
    /// will pick target with the least amount of condition fails.
    /// </summary>
    [DataField]
    public List<MegafaunaEntityCondition> TargetConditions = new();

    /// <summary>
    /// If true, instead of just picking a target with the biggest weight,
    /// weighted random will be run between all targets to determine the result.
    /// </summary>
    [DataField]
    public bool WeightedRandom;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;

        if (!entMan.TryGetComponent<AggressiveComponent>(args.Entity, out var aggressiveComp))
        {
            DebugTools.Assert($"Megafauna AI doesn't have {nameof(AggressiveComponent)}, but tried to pick a target using it's data!");
            return FailDelay;
        }

        if (aggressiveComp.Aggressors.Count == 0)
        {
            DebugTools.Assert($"Megafauna AI failed to pick a target from {nameof(AggressiveComponent)}, it doesn't have any targets to pick from.");
            return FailDelay;
        }

        // Check all conditions on all possible targets
        var results = new Dictionary<EntityUid, float>();
        foreach (var target in aggressiveComp.Aggressors)
        {
            var weight = 0f;
            foreach (var condition in TargetConditions)
            {
                weight += condition.Evaluate(args, target);
            }

            results.Add(target, weight);
        }

        EntityUid? picked = null;
        if (WeightedRandom)
            picked = SharedRandomExtensions.Pick(results, args.Random);
        else
        {
            var maxWeight = float.MinValue;
            foreach (var (target, fails) in results)
            {
                if (maxWeight < fails)
                {
                    maxWeight = fails;
                    picked = target;
                }
            }

            DebugTools.Assert(picked != null, nameof(picked) + " != null"); // It's impossible at that point, but better to check.
        }

        var comp = args.EntityManager.EnsureComponent<MegafaunaAiTargetingComponent>(args.Entity);

        comp.TargetEnt = picked.Value;
        comp.TargetCoords = args.EntityManager.GetComponent<TransformComponent>(picked.Value).Coordinates;

        return DelaySelector.Get(args);
    }
}
