using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class NoLimbsEntityConditionSystem : EntityConditionSystem<BodyComponent, NoLimbsCondition>
{
    [Dependency] private SharedBodySystem _body = default!;

    protected override void Condition(Entity<BodyComponent> entity, ref EntityConditionEvent<NoLimbsCondition> args)
    {
        foreach (var (_, part) in _body.GetBodyChildren(entity, entity.Comp))
        {
            if (part.PartType is BodyPartType.Arm or BodyPartType.Leg)
                return;
        }

        args.Result = true;
    }
}

public sealed partial class NoLimbsCondition : EntityConditionBase<NoLimbsCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
