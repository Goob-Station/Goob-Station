using Content.Shared.EntityConditions;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class PullingEntityConditionSystem : EntityConditionSystem<PullerComponent, PullingCondition>
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    protected override void Condition(Entity<PullerComponent> entity, ref EntityConditionEvent<PullingCondition> args)
    {
        args.Result = entity.Comp.Pulling is { } pulled && _conditions.TryConditions(pulled, args.Condition.Conditions);
    }
}

public sealed partial class PullingCondition : EntityConditionBase<PullingCondition>
{
    [DataField]
    public EntityCondition[]? Conditions;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
