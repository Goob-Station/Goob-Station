using Content.Shared.Body.Systems;
using Content.Shared.EntityConditions;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class ForeignBodyPartEntityConditionSystem : EntityConditionSystem<HumanoidAppearanceComponent, ForeignBodyPartCondition>
{
    [Dependency] private SharedBodySystem _body = default!;

    protected override void Condition(Entity<HumanoidAppearanceComponent> entity, ref EntityConditionEvent<ForeignBodyPartCondition> args)
    {
        foreach (var (_, part) in _body.GetBodyChildren(entity))
        {
            if (string.IsNullOrEmpty(part.Species) || part.Species == entity.Comp.Species.Id)
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class ForeignBodyPartCondition : EntityConditionBase<ForeignBodyPartCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
