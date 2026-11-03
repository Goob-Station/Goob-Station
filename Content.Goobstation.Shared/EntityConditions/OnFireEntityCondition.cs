using Content.Shared.Atmos.Components;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class OnFireEntityConditionSystem : EntityConditionSystem<FlammableComponent, OnFireCondition>
{
    protected override void Condition(Entity<FlammableComponent> entity, ref EntityConditionEvent<OnFireCondition> args)
    {
        args.Result = entity.Comp.OnFire;
    }
}

public sealed partial class OnFireCondition : EntityConditionBase<OnFireCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
