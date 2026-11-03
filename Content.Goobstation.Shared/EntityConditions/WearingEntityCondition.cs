using Content.Shared.EntityConditions;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class WearingEntityConditionSystem : EntityConditionSystem<InventoryComponent, WearingCondition>
{
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    protected override void Condition(Entity<InventoryComponent> entity, ref EntityConditionEvent<WearingCondition> args)
    {
        args.Result = _inventory.TryGetSlotEntity(entity, args.Condition.Slot, out var item, entity.Comp)
                      && _whitelist.IsWhitelistPass(args.Condition.Whitelist, item.Value);
    }
}

public sealed partial class WearingCondition : EntityConditionBase<WearingCondition>
{
    [DataField(required: true)]
    public string Slot = string.Empty;

    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
