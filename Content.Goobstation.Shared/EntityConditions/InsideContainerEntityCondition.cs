using Content.Shared.EntityConditions;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class InsideContainerEntityConditionSystem : EntityConditionSystem<TransformComponent, InsideContainerCondition>
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<InsideContainerCondition> args)
    {
        args.Result = _container.TryGetContainingContainer((entity, entity.Comp, null), out var container)
                      && _whitelist.IsWhitelistPass(args.Condition.Whitelist, container.Owner);
    }
}

public sealed partial class InsideContainerCondition : EntityConditionBase<InsideContainerCondition>
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
