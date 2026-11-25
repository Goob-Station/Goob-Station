using Content.Server.Shuttles.Systems;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityConditions;

public sealed partial class EvacuationArrivedEntityConditionSystem : EntityConditionSystem<TransformComponent, EvacuationArrivedCondition>
{
    [Dependency] private EmergencyShuttleSystem _emergency = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<EvacuationArrivedCondition> args)
    {
        args.Result = _emergency.EmergencyShuttleArrived && !_emergency.ShuttlesLeft;
    }
}

public sealed partial class EvacuationArrivedCondition : EntityConditionBase<EvacuationArrivedCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
