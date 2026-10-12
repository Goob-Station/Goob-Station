using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class HoldingEntityConditionSystem : EntityConditionSystem<HandsComponent, HoldingCondition>
{
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    protected override void Condition(Entity<HandsComponent> entity, ref EntityConditionEvent<HoldingCondition> args)
    {
        foreach (var held in _hands.EnumerateHeld((entity.Owner, entity.Comp)))
        {
            if (!_whitelist.IsWhitelistPassOrNull(args.Condition.Whitelist, held)
                || args.Condition.Prototypes is { } prototypes
                && (Prototype(held) is not { } proto || !prototypes.Contains(proto.ID)))
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class HoldingCondition : EntityConditionBase<HoldingCondition>
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public List<EntProtoId>? Prototypes;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
