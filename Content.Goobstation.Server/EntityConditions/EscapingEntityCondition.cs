using Content.Server.Shuttles.Systems;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityConditions;

public sealed partial class EscapingEntityConditionSystem : EntityConditionSystem<TransformComponent, EscapingCondition>
{
    [Dependency] private EmergencyShuttleSystem _emergency = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<EscapingCondition> args)
    {
        args.Result = _emergency.IsTargetEscaping(entity);
    }
}

public sealed partial class EscapingCondition : EntityConditionBase<EscapingCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
