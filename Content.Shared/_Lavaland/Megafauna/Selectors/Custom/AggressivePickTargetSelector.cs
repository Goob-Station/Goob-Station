using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;
using Content.Shared._Lavaland.Megafauna.Systems;

// ReSharper disable once CheckNamespace
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

    /// <summary>
    /// Sets the EntityUid field in <see cref="MegafaunaAiTargetingComponent"/> if true.
    /// </summary>
    [DataField]
    public bool SetTarget = true;

    /// <summary>
    /// Sets the EntityCoordinates field in <see cref="MegafaunaAiTargetingComponent"/> if true.
    /// </summary>
    [DataField]
    public bool SetCoordinates;

    /// <summary>
    /// If true, will clear all previous target values before assigning new ones.
    /// </summary>
    [DataField]
    public bool ClearAll = true;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var system = args.EntityManager.System<MegafaunaSystem>();

        if (!system.TryPickTargetAggressive(args, TargetConditions, SetTarget, SetCoordinates, ClearAll))
            return FailDelay;

        return DelaySelector.Get(args);
    }
}
