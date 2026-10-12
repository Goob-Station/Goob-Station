using Content.Shared.Cuffs.Components;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class CuffedEntityConditionSystem : EntityConditionSystem<CuffableComponent, CuffedCondition>
{
    protected override void Condition(Entity<CuffableComponent> entity, ref EntityConditionEvent<CuffedCondition> args)
    {
        args.Result = entity.Comp.CuffedHandCount > 0;
    }
}

public sealed partial class CuffedCondition : EntityConditionBase<CuffedCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
