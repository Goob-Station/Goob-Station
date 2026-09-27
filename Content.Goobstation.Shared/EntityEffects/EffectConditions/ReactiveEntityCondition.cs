using Content.Goobstation.Shared.Chemistry;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.EffectConditions;

public sealed partial class ReactiveCondition : EntityConditionBase<ReactiveCondition>
{
    [DataField(required: true)]
    public string? SolutionName;

    [DataField(required: true)]
    public Dictionary<ProtoId<ReagentPrototype>, MinMax> Reagents;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-");
    }
}

/// <summary>
/// Same as <see cref="ReagentEntityCondition"/> but subscribed on SolutionContainerManagerComponent instead
/// </summary>
public sealed partial class ReactiveEntityCondition : EntityConditionSystem<SolutionContainerManagerComponent, ReactiveCondition>
{
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    protected override void Condition(Entity<SolutionContainerManagerComponent> ent, ref EntityConditionEvent<ReactiveCondition> args)
    {
        if (!_solution.TryGetSolution(ent.AsNullable(), args.Condition.SolutionName, out var sln, out var _, true))
            return;

        if (sln is not { } solution || solution.Comp.Solution.Contents.Count == 0)
            return;

        var reagents = args.Condition.Reagents;
        var result = false;
        foreach (var reagent in reagents)
        {
            var min = reagent.Value.Min;
            var max = reagent.Value.Max;

            if (!solution.Comp.Solution.ContainsPrototype(reagent.Key))
            {
                result = false;
                continue;
            }

            var quantity = solution.Comp.Solution.GetTotalPrototypeQuantity(reagent.Key);
            result = quantity >= min && quantity <= max;
        }

        args.Result = result; // Did this so the condition don't return early if it find the correct reagent
    }
}
