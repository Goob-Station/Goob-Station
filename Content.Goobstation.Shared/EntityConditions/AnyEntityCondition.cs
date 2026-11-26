using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class AnyEntityConditionSystem : EntityConditionSystem<TransformComponent, AnyCondition>
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<AnyCondition> args)
    {
        args.Result = _conditions.TryAnyCondition(entity, args.Condition.Conditions);
    }
}

public sealed partial class AnyCondition : EntityConditionBase<AnyCondition>
{
    [DataField(required: true)]
    public EntityCondition[] Conditions = Array.Empty<EntityCondition>();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
