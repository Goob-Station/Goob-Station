using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class HandCountEntityConditionSystem : EntityConditionSystem<HandsComponent, HandCountCondition>
{
    protected override void Condition(Entity<HandsComponent> entity, ref EntityConditionEvent<HandCountCondition> args)
    {
        args.Result = entity.Comp.Count >= args.Condition.Min && entity.Comp.Count <= args.Condition.Max;
    }
}

public sealed partial class HandCountCondition : EntityConditionBase<HandCountCondition>
{
    [DataField]
    public int Min;

    [DataField]
    public int Max = int.MaxValue;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
