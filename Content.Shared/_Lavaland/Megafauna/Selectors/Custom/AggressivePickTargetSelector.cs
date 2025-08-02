using Content.Shared._Lavaland.Megafauna.Conditions;
using Content.Shared._Lavaland.Megafauna.Systems;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Uses AggressiveComponent to pick a new target to attack.
/// </summary>
public sealed partial class AggressivePickTargetSelector : MegafaunaSelector
{
    /// <summary>
    /// Checks that will run on all possible target entities. Then selector
    /// will pick target with the least amount of condition fails.
    /// </summary>
    [DataField]
    public List<MegafaunaTargetCondition> TargetConditions = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var system = args.EntityManager.System<MegafaunaSystem>();

        if (!system.TryPickTargetAggressive(args, TargetConditions))
            return FailDelay;

        return DelaySelector.Get(args);
    }
}
