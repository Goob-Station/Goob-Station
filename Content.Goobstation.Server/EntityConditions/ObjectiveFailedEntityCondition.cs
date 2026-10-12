using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityConditions;

public sealed partial class ObjectiveFailedEntityConditionSystem : EntityConditionSystem<MindContainerComponent, ObjectiveFailedCondition>
{
    [Dependency] private SharedObjectivesSystem _objectives = default!;

    protected override void Condition(Entity<MindContainerComponent> entity, ref EntityConditionEvent<ObjectiveFailedCondition> args)
    {
        if (entity.Comp.Mind is not { } mindId || !TryComp<MindComponent>(mindId, out var mind))
            return;

        foreach (var objective in mind.Objectives)
        {
            if (Prototype(objective)?.ID != args.Condition.Objective.Id
                || _objectives.IsCompleted(objective, (mindId, mind)))
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class ObjectiveFailedCondition : EntityConditionBase<ObjectiveFailedCondition>
{
    [DataField(required: true)]
    public EntProtoId Objective;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
