using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class GrabStageEntityConditionSystem : EntityConditionSystem<GrabIntentComponent, GrabStageCondition>
{
    protected override void Condition(Entity<GrabIntentComponent> ent, ref EntityConditionEvent<GrabStageCondition> args)
    {
        args.Result = args.Condition.Stage == ent.Comp.GrabStage;
    }
}

public sealed partial class GrabStageCondition : EntityConditionBase<GrabStageCondition>
{
    [DataField(required: true)]
    public GrabStage Stage;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-grabstage", ("inverted", Inverted));
    }
}
