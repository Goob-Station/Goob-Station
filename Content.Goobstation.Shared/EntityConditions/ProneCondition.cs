using Content.Shared.EntityConditions;
using Content.Shared.Standing;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class ProneEntityConditionSystem : EntityConditionSystem<TransformComponent, ProneCondition>
{
    [Dependency] private StandingStateSystem _standingState = default!;

    protected override void Condition(Entity<TransformComponent> ent, ref EntityConditionEvent<ProneCondition> args)
    {
        args.Result = _standingState.IsDown(ent.Owner);
    }
}

public sealed partial class ProneCondition : EntityConditionBase<ProneCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-prone", ("inverted", Inverted));
    }
}
