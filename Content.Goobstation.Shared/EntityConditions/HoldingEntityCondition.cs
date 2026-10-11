using Content.Shared.EntityConditions;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class HoldingEntityConditionSystem : EntityConditionSystem<HandsComponent, HoldingCondition>
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    protected override void Condition(Entity<HandsComponent> entity, ref EntityConditionEvent<HoldingCondition> args)
    {
        foreach (var held in _hands.EnumerateHeld((entity.Owner, entity.Comp)))
        {
            if (!_whitelist.IsWhitelistPass(args.Condition.Whitelist, held))
                continue;

            args.Result = true;
            return;
        }
    }
}

public sealed partial class HoldingCondition : EntityConditionBase<HoldingCondition>
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
