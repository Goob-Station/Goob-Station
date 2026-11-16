using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityConditions;

public sealed partial class ObjectivesCompletedEntityConditionSystem : EntityConditionSystem<MindContainerComponent, ObjectivesCompletedCondition>
{
    [Dependency] private SharedObjectivesSystem _objectives = default!;

    protected override void Condition(Entity<MindContainerComponent> entity, ref EntityConditionEvent<ObjectivesCompletedCondition> args)
    {
        if (entity.Comp.Mind is not { } mindId
            || !TryComp<MindComponent>(mindId, out var mind)
            || mind.Objectives.Count == 0)
            return;

        foreach (var objective in mind.Objectives)
        {
            if (!_objectives.IsCompleted(objective, (mindId, mind)))
                return;
        }

        args.Result = true;
    }
}

public sealed partial class ObjectivesCompletedCondition : EntityConditionBase<ObjectivesCompletedCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
